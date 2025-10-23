using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MovieApp.Api.Exceptions;
using MovieApp.Api.Domain;
using MovieApp.Api.Configuration;
using MovieApp.Api.Providers;

namespace MovieApp.Api.Services
{
    public class MovieAggregatorService : IMovieAggregatorService
    {
        private readonly IEnumerable<IMovieProviderClient> _providers;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MovieAggregatorService> _logger;
        private readonly TimeSpan _listTtl;
        private readonly TimeSpan _detailTtl;
        private readonly int _maxDetailConcurrency;

        public MovieAggregatorService(
            IEnumerable<IMovieProviderClient> providers,
            IMemoryCache cache,
            ILogger<MovieAggregatorService> logger,
            IOptions<AggregatorOptions> options)
        {
            _providers = providers;
            _cache = cache;
            _logger = logger;

            var o = options.Value ?? new AggregatorOptions();
            _listTtl = TimeSpan.FromMinutes(o.ListTtlMinutes > 0 ? o.ListTtlMinutes : 3);
            _detailTtl = TimeSpan.FromMinutes(o.DetailTtlMinutes > 0 ? o.DetailTtlMinutes : 10);
            _maxDetailConcurrency = Math.Max(2, o.MaxDetailConcurrency);
        }

        public async Task<MoviesResponse> GetMoviesAsync(CancellationToken ct)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var listTasks = _providers.Select(p => GetListSafe(p, ct)).ToArray();
            var results = await Task.WhenAll(listTasks);

            var byProvider = new Dictionary<string, List<MovieSummary>>(StringComparer.OrdinalIgnoreCase);
            var allMovies = new List<MovieSummary>();

            foreach (var (provider, list, error) in results)
            {
                if (list is { Count: > 0 })
                {
                    byProvider[provider] = list;
                    allMovies.AddRange(list);
                    _logger.LogInformation("Provider {Provider}: {Count} movies", provider, list.Count);
                }
                else if (error != null)
                {
                    _logger.LogWarning("Provider {Provider} failed: {Error}", provider, error);
                }
            }

            if (allMovies.Count == 0)
            {
                _logger.LogWarning("No movies found from any provider");
                return new MoviesResponse { Movies = new List<MovieItemResponse>() };
            }

            var moviesByTitle = allMovies
                .GroupBy(m => m.Title, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList());

            var responseMap = new ConcurrentDictionary<string, MovieItemResponse>(StringComparer.OrdinalIgnoreCase);

            using var semaphore = new SemaphoreSlim(_maxDetailConcurrency, _maxDetailConcurrency);
            var detailTasks = new List<Task>();

            foreach (var kvp in moviesByTitle)
            {
                var title = kvp.Key;
                var movies = kvp.Value;
                
                var firstMovie = movies.First();
                var providerIds = new Dictionary<string, string>();
                foreach (var movie in movies)
                {
                    var providerName = GetProviderName(movie.ID);
                    providerIds[providerName] = movie.ID;
                }
                
                responseMap[title] = new MovieItemResponse
                {
                    Id = firstMovie.ID,
                    Title = title,
                    Year = firstMovie.Year,
                    Type = firstMovie.Type,
                    Poster = firstMovie.Poster,
                    PricesByProvider = new Dictionary<string, decimal>(),
                    ProviderIds = providerIds
                };

                foreach (var movie in movies)
                {
                    var providerName = GetProviderName(movie.ID);
                    detailTasks.Add(FetchMovieDetailAsync(providerName, movie.ID, title, semaphore, responseMap, ct));
                }
            }

            await Task.WhenAll(detailTasks);
            foreach (var movie in responseMap.Values)
            {
                if (movie.PricesByProvider.Count > 0)
                {
                    var cheapest = movie.PricesByProvider.MinBy(kvp => kvp.Value);
                    movie.CheapestPrice = cheapest.Value;
                    movie.CheapestProvider = cheapest.Key;
                }
            }

            var finalMovies = responseMap.Values
                .Where(m => m.PricesByProvider.Count > 0)
                .OrderBy(m => m.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            stopwatch.Stop();
            _logger.LogInformation("Returning {Count} movies in {ElapsedMs}ms", finalMovies.Count, stopwatch.ElapsedMilliseconds);

            return new MoviesResponse { Movies = finalMovies };
        }

        private async Task FetchMovieDetailAsync(
            string providerName,
            string movieId,
            string title,
            SemaphoreSlim semaphore,
            ConcurrentDictionary<string, MovieItemResponse> responseMap,
            CancellationToken ct)
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var detail = await GetDetailWithCache(providerName, movieId, ct);
                if (detail != null)
                {
                    responseMap.AddOrUpdate(title,
                        _ => new MovieItemResponse
                        {
                            Id = title,
                            Title = title,
                            Year = detail.Year,
                            Type = "",
                            Poster = detail.Poster,
                            CheapestPrice = detail.Price,
                            CheapestProvider = providerName,
                            PricesByProvider = new Dictionary<string, decimal> { [providerName] = detail.Price }
                        },
                        (_, existing) =>
                        {
                            existing.PricesByProvider[providerName] = detail.Price;
                            return existing;
                        });
                }
                else
                {
                    _logger.LogWarning("Detail is null for {Provider}:{Id}", providerName, movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Detail fetch failed for {Provider}:{Id} - {Message}", providerName, movieId, ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private string GetProviderName(string movieId)
        {
            return movieId.StartsWith("cw") ? "cinemaworld" : "filmworld";
        }

        private async Task<(string provider, List<MovieSummary>? list, string? error)> GetListSafe(
            IMovieProviderClient p, CancellationToken ct)
        {
            try
            {
                var list = await GetListCached(p, ct);
                return (p.Name, list, null);
            }
            catch (ProviderException ex)
            {
                return (p.Name, null, ex.Message);
            }
            catch (Exception ex)
            {
                return (p.Name, null, $"Unexpected error: {ex.Message}");
            }
        }

        private async Task<List<MovieSummary>> GetListCached(IMovieProviderClient p, CancellationToken ct)
        {
            var key = $"list:{p.Name}";
            if (_cache.TryGetValue(key, out List<MovieSummary>? cached) && cached != null)
                return cached;

            var list = await p.GetMoviesAsync(ct);
            _cache.Set(key, list, _listTtl);
            return list;
        }

        private async Task<MovieDetail?> GetDetailWithCache(string provider, string id, CancellationToken ct)
        {
            var key = $"detail:{provider}:{id}";
            if (_cache.TryGetValue(key, out MovieDetail? cached))
                return cached;

            try
            {
                var src = _providers.First(x => string.Equals(x.Name, provider, StringComparison.OrdinalIgnoreCase));
                var d = await src.GetMovieAsync(id, ct);
                if (d != null)
                {
                    _cache.Set(key, d, _detailTtl);
                }
                return d;
            }
            catch (ProviderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ProviderException(provider, "Unexpected error getting movie detail", ex);
            }
        }

        public async Task<MovieDetailResponse?> GetMovieAsync(string id, CancellationToken ct)
        {
            var moviesResponse = await GetMoviesAsync(ct);
            var movie = moviesResponse.Movies.FirstOrDefault(m => 
                string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
            
            if (movie == null)
                throw new MovieNotFoundException(id);

            var res = new MovieDetailResponse 
            { 
                Id = movie.Id, 
                Title = movie.Title,
                Year = movie.Year,
                Type = movie.Type,
                Poster = movie.Poster,
                CheapestPrice = movie.CheapestPrice,
                CheapestProvider = movie.CheapestProvider,
                PricesByProvider = new Dictionary<string, decimal>(movie.PricesByProvider)
            };

            foreach (var kvp in movie.ProviderIds)
            {
                var providerName = kvp.Key;
                var providerId = kvp.Value;
                
                try
                {
                    var d = await GetDetailWithCache(providerName, providerId, ct);
                    if (d != null)
                    {
                        if (string.IsNullOrEmpty(res.Rated)) res.Rated = d.Rated;
                        if (string.IsNullOrEmpty(res.Released)) res.Released = d.Released;
                        if (string.IsNullOrEmpty(res.Runtime)) res.Runtime = d.Runtime;
                        if (string.IsNullOrEmpty(res.Genre)) res.Genre = d.Genre;
                        if (string.IsNullOrEmpty(res.Director)) res.Director = d.Director;
                        if (string.IsNullOrEmpty(res.Actors)) res.Actors = d.Actors;
                        if (string.IsNullOrEmpty(res.Plot)) res.Plot = d.Plot;
                        if (string.IsNullOrEmpty(res.Poster)) res.Poster = d.Poster;

                        res.PricesByProvider[providerName] = d.Price;
                        if (res.CheapestPrice == null || d.Price < res.CheapestPrice.Value)
                        {
                            res.CheapestPrice = d.Price;
                            res.CheapestProvider = providerName;
                        }
                    }
                }
                catch
                {
                }
            }

            return res;
        }
    }
}