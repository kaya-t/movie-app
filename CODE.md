# 🎬 Movie App - Complete Code Documentation

A comprehensive .NET 8.0 Web API with React frontend that aggregates movie data from multiple providers (Cinemaworld and Filmworld) with advanced features including resilient HTTP calls, centralized exception handling, structured logging, memory caching, and price comparison.

## 📁 Project Structure

```
movie-app/
├── src/
│   ├── api/                           # .NET Web API
│   │   ├── api.tests/                 # Backend tests (nested inside api)
│   │   │   ├── Integration/
│   │   │   │   └── MoviesControllerIntegrationTests.cs
│   │   │   ├── Unit/
│   │   │   │   ├── Controllers/
│   │   │   │   │   └── MoviesControllerTests.cs
│   │   │   │   ├── Filters/
│   │   │   │   │   └── GlobalExceptionFilterTests.cs
│   │   │   │   └── Services/
│   │   │   │       └── MovieAggregatorServiceTests.cs
│   │   │   ├── Utilities/
│   │   │   │   └── TestDataBuilder.cs
│   │   │   └── MovieApp.Api.Tests.csproj
│   │   ├── Configuration/
│   │   │   ├── AggregatorOptions.cs
│   │   │   └── ProviderOptions.cs
│   │   ├── Controllers/
│   │   │   └── MoviesController.cs
│   │   ├── Domain/
│   │   │   ├── ApiModels.cs
│   │   │   ├── ProviderDtos.cs
│   │   │   └── ValueObjects.cs
│   │   ├── Exceptions/
│   │   │   ├── MovieNotFoundException.cs
│   │   │   └── ProviderException.cs
│   │   ├── Filters/
│   │   │   └── GlobalExceptionFilter.cs
│   │   ├── Providers/
│   │   │   ├── CinemaworldClient.cs
│   │   │   ├── FilmworldClient.cs
│   │   │   └── IMovieProviderClient.cs
│   │   ├── Services/
│   │   │   ├── Caching/
│   │   │   │   └── CacheKeys.cs
│   │   │   ├── IMovieAggregatorService.cs
│   │   │   └── MovieAggregatorService.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── MovieApp.Api.csproj
│   │   └── Program.cs
│   └── ui/                            # React app
│       ├── public/
│       │   └── index.html
│       ├── src/
│       │   ├── __tests__/
│       │   │   ├── App.test.js
│       │   │   ├── MovieDetail.test.js
│       │   │   ├── MovieList.test.js
│       │   │   ├── api.test.js
│       │   │   └── utils.test.js
│       │   ├── components/
│       │   │   ├── MovieDetail.js
│       │   │   └── MovieList.js
│       │   ├── services/
│       │   │   └── api.js
│       │   ├── App.js
│       │   ├── index.css
│       │   ├── index.js
│       │   └── setupTests.js
│       ├── package.json
│       ├── package-lock.json
│       └── README.md
├── README.md
└── CODE.md
```

## 🔧 Backend API Code

### Program.cs
```csharp
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using MovieApp.Api.Domain;
using MovieApp.Api.Configuration;
using MovieApp.Api.Providers;
using MovieApp.Api.Services;
using MovieApp.Api.Filters;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod())
);

builder.Services.Configure<ProviderOptions>(builder.Configuration.GetSection("Providers"));
builder.Services.Configure<AggregatorOptions>(builder.Configuration.GetSection("Webjet"));

var jitterDelays = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(150), retryCount: 2);
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(jitterDelays);
var perTryTimeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3));
var policyWrap = Policy.WrapAsync(retryPolicy, perTryTimeout);

Action<IServiceProvider, HttpClient> configureHttp = (sp, http) =>
{
    http.Timeout = TimeSpan.FromSeconds(5);
    http.DefaultRequestHeaders.UserAgent.ParseAdd("MovieApp.Api/1.0");
    http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    http.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
};

Func<HttpMessageHandler> handlerFactory = () => new SocketsHttpHandler
{
    AutomaticDecompression = DecompressionMethods.GZip
                           | DecompressionMethods.Deflate
                           | DecompressionMethods.Brotli,
    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
};

builder.Services
  .AddHttpClient<CinemaworldClient>(configureHttp)
  .ConfigurePrimaryHttpMessageHandler(handlerFactory)
  .AddPolicyHandler(policyWrap);

builder.Services
  .AddHttpClient<FilmworldClient>(configureHttp)
  .ConfigurePrimaryHttpMessageHandler(handlerFactory)
  .AddPolicyHandler(policyWrap);

builder.Services.AddScoped<IMovieProviderClient, CinemaworldClient>();
builder.Services.AddScoped<IMovieProviderClient, FilmworldClient>();
builder.Services.AddScoped<IMovieAggregatorService, MovieAggregatorService>();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

public partial class Program { }
```

### Controllers/MoviesController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using MovieApp.Api.Domain;
using MovieApp.Api.Services;
using MovieApp.Api.Exceptions;

namespace MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieAggregatorService _svc;

        public MoviesController(IMovieAggregatorService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<ActionResult<MoviesResponse>> GetMovies(CancellationToken ct)
        {
            var data = await _svc.GetMoviesAsync(ct);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieDetailResponse>> GetMovie(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Movie ID is required" });

            var data = await _svc.GetMovieAsync(id, ct);
            if (data == null)
                throw new MovieNotFoundException(id);

            return Ok(data);
        }
    }
}
```

### Services/MovieAggregatorService.cs
```csharp
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
            _logger.LogInformation("Returning {Count} movies in {Elapsed}ms", finalMovies.Count, stopwatch.ElapsedMilliseconds);

            return new MoviesResponse { Movies = finalMovies };
        }

        private async Task<(string provider, List<MovieSummary>? list, string? error)> GetListSafe(IMovieProviderClient p, CancellationToken ct)
        {
            try
            {
                var list = await GetListCached(p, ct);
                return (p.Name, list, null);
            }
            catch (Exception ex)
            {
                return (p.Name, null, ex.Message);
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

        private async Task<MovieDetail?> GetDetailWithCache(string providerName, string id, CancellationToken ct)
        {
            var key = $"detail:{providerName}:{id}";
            if (_cache.TryGetValue(key, out MovieDetail? cached) && cached != null)
                return cached;

            var detail = await FetchDetailSafe(providerName, id, ct);
            if (detail != null)
            {
                _cache.Set(key, detail, _detailTtl);
            }
            return detail;
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
                    _logger.LogWarning("Detail is null for {Provider}:{MovieId}", providerName, movieId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to fetch detail for {Provider}:{MovieId}. Error: {Error}", providerName, movieId, ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<MovieDetail?> FetchDetailSafe(string provider, string id, CancellationToken ct)
        {
            try
            {
                var p = _providers.First(x => x.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
                return await p.GetMovieAsync(id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie detail from {Provider} for ID {Id}", provider, id);
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

        private string GetProviderName(string movieId)
        {
            if (movieId.StartsWith("cw", StringComparison.OrdinalIgnoreCase)) return "cinemaworld";
            if (movieId.StartsWith("fw", StringComparison.OrdinalIgnoreCase)) return "filmworld";
            throw new ArgumentException("Unknown movie ID format", nameof(movieId));
        }
    }
}
```

### Domain/ApiModels.cs
```csharp
namespace MovieApp.Api.Domain
{
    public class MovieItemResponse
    {
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Type { get; set; } = "";
        public string Poster { get; set; } = "";

        public string Id { get; set; } = "";

        public decimal? CheapestPrice { get; set; }
        public string? CheapestProvider { get; set; }
        public Dictionary<string, decimal> PricesByProvider { get; set; } = new();
        
        public Dictionary<string, string> ProviderIds { get; set; } = new();
    }

    public class MoviesResponse
    {
        public List<MovieItemResponse> Movies { get; set; } = new();
    }

    public class MovieDetailResponse : MovieItemResponse
    {
        public string Rated { get; set; } = "";
        public string Released { get; set; } = "";
        public string Runtime { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Director { get; set; } = "";
        public string Actors { get; set; } = "";
        public string Plot { get; set; } = "";
    }
}
```

### Domain/ProviderDtos.cs
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MovieApp.Api.Domain
{
    public class MovieListResponse
    {
        public List<MovieSummary> Movies { get; set; } = new();
    }

    public class MovieSummary
    {
        public string ID { get; set; } = "";
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Type { get; set; } = "";
        public string Poster { get; set; } = "";
    }

    public class MovieDetail
    {
        public string ID { get; set; } = "";
        public string Title { get; set; } = "";
        public string Year { get; set; } = "";
        public string Rated { get; set; } = "";
        public string Released { get; set; } = "";
        public string Runtime { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Writer { get; set; } = "";
        public string Actors { get; set; } = "";
        public string Plot { get; set; } = "";
        public string Language { get; set; } = "";
        public string Country { get; set; } = "";
        public string Awards { get; set; } = "";
        public string Poster { get; set; } = "";
        public string Metascore { get; set; } = "";
        public string Rating { get; set; } = "";
        public string Votes { get; set; } = "";
        public string Type { get; set; } = "";
        
        [JsonConverter(typeof(DecimalStringConverter))]
        public decimal Price { get; set; }
    }

    public class DecimalStringConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (decimal.TryParse(reader.GetString(), out decimal result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }
            
            throw new JsonException($"Unable to convert to decimal. TokenType: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
```

### Providers/CinemaworldClient.cs
```csharp
using System.Text.Json;
using Microsoft.Extensions.Options;
using MovieApp.Api.Configuration;
using MovieApp.Api.Domain;

namespace MovieApp.Api.Providers
{
    public class CinemaworldClient : IMovieProviderClient
    {
        private readonly HttpClient _httpClient;
        private readonly ProviderConfig _config;

        public string Name => "cinemaworld";

        public CinemaworldClient(HttpClient httpClient, IOptions<ProviderOptions> options)
        {
            _httpClient = httpClient;
            _config = options.Value.Cinemaworld;
        }

        public async Task<List<MovieSummary>> GetMoviesAsync(CancellationToken ct)
        {
            var response = await _httpClient.GetAsync($"{_config.BaseUrl}/movies", ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<MovieListResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result?.Movies ?? new List<MovieSummary>();
        }

        public async Task<MovieDetail?> GetMovieAsync(string id, CancellationToken ct)
        {
            var response = await _httpClient.GetAsync($"{_config.BaseUrl}/movie/{id}", ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<MovieDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result;
        }
    }
}
```

### Providers/FilmworldClient.cs
```csharp
using System.Text.Json;
using Microsoft.Extensions.Options;
using MovieApp.Api.Configuration;
using MovieApp.Api.Domain;

namespace MovieApp.Api.Providers
{
    public class FilmworldClient : IMovieProviderClient
    {
        private readonly HttpClient _httpClient;
        private readonly ProviderConfig _config;

        public string Name => "filmworld";

        public FilmworldClient(HttpClient httpClient, IOptions<ProviderOptions> options)
        {
            _httpClient = httpClient;
            _config = options.Value.Filmworld;
        }

        public async Task<List<MovieSummary>> GetMoviesAsync(CancellationToken ct)
        {
            var response = await _httpClient.GetAsync($"{_config.BaseUrl}/movies", ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<MovieListResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result?.Movies ?? new List<MovieSummary>();
        }

        public async Task<MovieDetail?> GetMovieAsync(string id, CancellationToken ct)
        {
            var response = await _httpClient.GetAsync($"{_config.BaseUrl}/movie/{id}", ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<MovieDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return result;
        }
    }
}
```

### Exceptions/MovieNotFoundException.cs
```csharp
namespace MovieApp.Api.Exceptions
{
    public class MovieNotFoundException : Exception
    {
        public MovieNotFoundException(string movieId) 
            : base($"Movie with ID '{movieId}' not found in any provider")
        {
        }
    }
}
```

### Exceptions/ProviderException.cs
```csharp
namespace MovieApp.Api.Exceptions
{
    public class ProviderException : Exception
    {
        public string ProviderName { get; }

        public ProviderException(string providerName, string message) 
            : base(message)
        {
            ProviderName = providerName;
        }

        public ProviderException(string providerName, string message, Exception innerException) 
            : base(message, innerException)
        {
            ProviderName = providerName;
        }
    }
}
```

### Filters/GlobalExceptionFilter.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MovieApp.Api.Exceptions;

namespace MovieApp.Api.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occurred: {Message}", context.Exception.Message);

            var result = context.Exception switch
            {
                MovieNotFoundException => new ObjectResult(new { message = context.Exception.Message }) { StatusCode = 404 },
                ProviderException => new ObjectResult(new { message = "Service temporarily unavailable" }) { StatusCode = 503 },
                _ => new ObjectResult(new { message = "An error occurred" }) { StatusCode = 500 }
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
```

## 🎨 Frontend UI Code

### src/App.js
```javascript
import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import MovieList from './components/MovieList';
import MovieDetail from './components/MovieDetail';
import './index.css';

function App() {
  return (
    <Router>
      <div className="App">
        <header className="App-header">
          <h1>Movie App</h1>
        </header>
        <main>
          <Routes>
            <Route path="/" element={<MovieList />} />
            <Route path="/movie/:id" element={<MovieDetail />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
```

### src/components/MovieList.js
```javascript
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getMovies } from '../services/api';

function MovieList() {
  const [movies, setMovies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchMovies = async () => {
      try {
        setLoading(true);
        const data = await getMovies();
        setMovies(data.movies || []);
      } catch (err) {
        setError('Failed to load movies');
        console.error('Error fetching movies:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchMovies();
  }, []);

  if (loading) return <div className="loading">Loading movies...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="movie-list">
      <h2>Movies</h2>
      <div className="movies-grid">
        {movies.map((movie) => (
          <div key={movie.id} className="movie-card">
            <Link to={`/movie/${movie.id}`}>
              <img src={movie.poster} alt={movie.title} />
              <h3>{movie.title}</h3>
              <p>Year: {movie.year}</p>
              {movie.cheapestPrice && (
                <p className="price">
                  From ${movie.cheapestPrice} ({movie.cheapestProvider})
                </p>
              )}
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}

export default MovieList;
```

### src/components/MovieDetail.js
```javascript
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getMovie } from '../services/api';

function MovieDetail() {
  const { id } = useParams();
  const [movie, setMovie] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchMovie = async () => {
      try {
        setLoading(true);
        const data = await getMovie(id);
        setMovie(data);
      } catch (err) {
        setError('Failed to load movie details');
        console.error('Error fetching movie:', err);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchMovie();
    }
  }, [id]);

  if (loading) return <div className="loading">Loading movie details...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!movie) return <div className="error">Movie not found</div>;

  return (
    <div className="movie-detail">
      <Link to="/" className="back-link">← Back to Movies</Link>
      
      <div className="movie-info">
        <img src={movie.poster} alt={movie.title} className="movie-poster" />
        <div className="movie-details">
          <h1>{movie.title}</h1>
          <p><strong>Year:</strong> {movie.year}</p>
          <p><strong>Type:</strong> {movie.type}</p>
          
          {movie.rated && <p><strong>Rated:</strong> {movie.rated}</p>}
          {movie.released && <p><strong>Released:</strong> {movie.released}</p>}
          {movie.runtime && <p><strong>Runtime:</strong> {movie.runtime}</p>}
          {movie.genre && <p><strong>Genre:</strong> {movie.genre}</p>}
          {movie.director && <p><strong>Director:</strong> {movie.director}</p>}
          {movie.actors && <p><strong>Actors:</strong> {movie.actors}</p>}
          {movie.plot && <p><strong>Plot:</strong> {movie.plot}</p>}
          
          {movie.cheapestPrice && (
            <div className="pricing">
              <h3>Pricing</h3>
              <p className="cheapest-price">
                Best Price: ${movie.cheapestPrice} ({movie.cheapestProvider})
              </p>
              
              {movie.pricesByProvider && Object.keys(movie.pricesByProvider).length > 0 && (
                <div className="all-prices">
                  <h4>All Prices:</h4>
                  <ul>
                    {Object.entries(movie.pricesByProvider).map(([provider, price]) => (
                      <li key={provider}>
                        {provider}: ${price}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default MovieDetail;
```

### src/services/api.js
```javascript
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

export const getMovies = async () => {
  const response = await fetch(`${API_BASE_URL}/movies`);
  if (!response.ok) {
    throw new Error('Failed to fetch movies');
  }
  return response.json();
};

export const getMovie = async (id) => {
  const response = await fetch(`${API_BASE_URL}/movies/${id}`);
  if (!response.ok) {
    throw new Error('Failed to fetch movie');
  }
  return response.json();
};
```

## 🧪 Test Code

### Backend Tests

#### Unit/Services/MovieAggregatorServiceTests.cs
```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MovieApp.Api.Configuration;
using MovieApp.Api.Domain;
using MovieApp.Api.Exceptions;
using MovieApp.Api.Providers;
using MovieApp.Api.Services;
using MovieApp.Api.Tests.Utilities;
using Xunit;

namespace MovieApp.Api.Tests.Unit.Services
{
    public class MovieAggregatorServiceTests
    {
        private readonly Mock<IMovieProviderClient> _mockProvider1;
        private readonly Mock<IMovieProviderClient> _mockProvider2;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<MovieAggregatorService>> _mockLogger;
        private readonly Mock<IOptions<AggregatorOptions>> _mockOptions;
        private readonly MovieAggregatorService _service;

        public MovieAggregatorServiceTests()
        {
            _mockProvider1 = new Mock<IMovieProviderClient>();
            _mockProvider1.Setup(x => x.Name).Returns("cinemaworld");

            _mockProvider2 = new Mock<IMovieProviderClient>();
            _mockProvider2.Setup(x => x.Name).Returns("filmworld");

            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<MovieAggregatorService>>();
            _mockOptions = new Mock<IOptions<AggregatorOptions>>();

            _mockOptions.Setup(x => x.Value).Returns(new AggregatorOptions
            {
                ListTtlMinutes = 3,
                DetailTtlMinutes = 10,
                MaxDetailConcurrency = 2
            });

            var providers = new[] { _mockProvider1.Object, _mockProvider2.Object };
            _service = new MovieAggregatorService(providers, _mockCache.Object, _mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task GetMoviesAsync_ShouldReturnAggregatedMovies_WhenProvidersReturnData()
        {
            // Arrange
            var cinemaworldMovies = TestDataBuilder.GetCinemaworldMovies().Take(2).ToList();
            var filmworldMovies = TestDataBuilder.GetFilmworldMovies().Take(2).ToList();

            var movie1Detail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0076759", 123.5m);
            var movie2Detail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0080684", 125.0m);
            var movie3Detail = TestDataBuilder.CreateFilmworldStarWarsDetail("fw0076759", 29.5m);

            _mockProvider1.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cinemaworldMovies);
            _mockProvider2.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(filmworldMovies);

            _mockProvider1.Setup(x => x.GetMovieAsync("cw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie1Detail);
            _mockProvider1.Setup(x => x.GetMovieAsync("cw0080684", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie2Detail);
            _mockProvider2.Setup(x => x.GetMovieAsync("fw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie3Detail);
            _mockProvider2.Setup(x => x.GetMovieAsync("fw0080684", It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestDataBuilder.CreateFilmworldStarWarsDetail("fw0080684", 31.0m));

            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
                .Returns(false);
            
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<Microsoft.Extensions.Caching.Memory.ICacheEntry>());

            // Act
            var result = await _service.GetMoviesAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Movies.Should().HaveCount(2);
            result.Movies.Should().Contain(m => m.Title == "Star Wars: Episode IV - A New Hope" && m.CheapestPrice == 29.5m);
            result.Movies.Should().Contain(m => m.Title == "Star Wars: Episode V - The Empire Strikes Back" && m.CheapestPrice == 31.0m);
        }

        [Fact]
        public async Task GetMoviesAsync_ShouldHandleProviderFailures_WhenOneProviderFails()
        {
            // Arrange
            var cinemaworldMovies = TestDataBuilder.GetCinemaworldMovies().Take(1).ToList();

            _mockProvider1.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cinemaworldMovies);
            _mockProvider2.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProviderException("filmworld", "Network error"));

            var movieDetail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0076759", 123.5m);
            _mockProvider1.Setup(x => x.GetMovieAsync("cw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movieDetail);

            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
                .Returns(false);
            
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<Microsoft.Extensions.Caching.Memory.ICacheEntry>());

            // Act
            var result = await _service.GetMoviesAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Movies.Should().HaveCount(1);
            result.Movies.Should().Contain(m => m.Title == "Star Wars: Episode IV - A New Hope");
        }

        [Fact]
        public async Task GetMovieAsync_ShouldReturnMovieDetails_WhenMovieExists()
        {
            // Arrange
            var cinemaworldMovies = TestDataBuilder.GetCinemaworldMovies().Take(1).ToList();
            var filmworldMovies = TestDataBuilder.GetFilmworldMovies().Take(1).ToList();
            
            var movieDetail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0076759", 123.5m);
            
            _mockProvider1.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cinemaworldMovies);
            _mockProvider2.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(filmworldMovies);
                
            _mockProvider1.Setup(x => x.GetMovieAsync("cw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movieDetail);

            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny))
                .Returns(false);
            
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<Microsoft.Extensions.Caching.Memory.ICacheEntry>());

            // Act
            var result = await _service.GetMovieAsync("cw0076759", CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Star Wars: Episode IV - A New Hope");
            result.CheapestPrice.Should().Be(123.5m);
            result.CheapestProvider.Should().Be("cinemaworld");
        }

        [Fact]
        public async Task GetMovieAsync_ShouldThrowMovieNotFoundException_WhenMovieNotFound()
        {
            // Arrange
            _mockProvider1.Setup(x => x.GetMovieAsync("nonexistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((MovieDetail?)null);
            _mockProvider2.Setup(x => x.GetMovieAsync("nonexistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((MovieDetail?)null);

            _mockProvider1.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MovieSummary>());
            _mockProvider2.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MovieSummary>());

            // Act
            await Assert.ThrowsAsync<MovieNotFoundException>(() => 
                _service.GetMovieAsync("nonexistent", CancellationToken.None));
        }

        [Fact]
        public async Task GetMoviesAsync_ShouldUseCache_WhenDataIsCached()
        {
            // Arrange
            var cachedMoviesCinemaworld = TestDataBuilder.GetCinemaworldMovies().Take(1).ToList();
            var cachedMoviesFilmworld = new List<MovieSummary>();

            _mockCache.Setup(x => x.TryGetValue("list:cinemaworld", out It.Ref<object?>.IsAny))
                .Returns((string key, out object? value) => {
                    value = cachedMoviesCinemaworld;
                    return true;
                });

            _mockCache.Setup(x => x.TryGetValue("list:filmworld", out It.Ref<object?>.IsAny))
                .Returns((string key, out object? value) => {
                    value = cachedMoviesFilmworld;
                    return true;
                });

            var cachedMovieDetail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0076759", 123.5m);
            _mockCache.Setup(x => x.TryGetValue("detail:cinemaworld:cw0076759", out It.Ref<object?>.IsAny))
                .Returns((string key, out object? value) => {
                    value = cachedMovieDetail;
                    return true;
                });

            // Act
            var result = await _service.GetMoviesAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Movies.Should().HaveCount(1);
            result.Movies.Should().Contain(m => m.Title == "Star Wars: Episode IV - A New Hope");
            
            _mockProvider1.Verify(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockProvider2.Verify(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
```

### Frontend Tests

#### __tests__/App.test.js
```javascript
import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import App from '../App';

const renderWithRouter = (component) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

test('renders movie app title', () => {
  renderWithRouter(<App />);
  const titleElement = screen.getByText(/Movie App/i);
  expect(titleElement).toBeInTheDocument();
});

test('renders movie list on home page', () => {
  renderWithRouter(<App />);
  const movieListElement = screen.getByText(/Movies/i);
  expect(movieListElement).toBeInTheDocument();
});
```

#### __tests__/MovieList.test.js
```javascript
import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import MovieList from '../components/MovieList';
import * as api from '../services/api';

jest.mock('../services/api');

const renderWithRouter = (component) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

test('renders loading state initially', () => {
  api.getMovies.mockResolvedValue({ movies: [] });
  renderWithRouter(<MovieList />);
  expect(screen.getByText(/Loading movies/i)).toBeInTheDocument();
});

test('renders movies when data is loaded', async () => {
  const mockMovies = [
    {
      id: '1',
      title: 'Test Movie',
      year: '2023',
      poster: 'test.jpg',
      cheapestPrice: 10.99,
      cheapestProvider: 'test-provider'
    }
  ];
  
  api.getMovies.mockResolvedValue({ movies: mockMovies });
  renderWithRouter(<MovieList />);
  
  await waitFor(() => {
    expect(screen.getByText('Test Movie')).toBeInTheDocument();
  });
});

test('renders error message when API fails', async () => {
  api.getMovies.mockRejectedValue(new Error('API Error'));
  renderWithRouter(<MovieList />);
  
  await waitFor(() => {
    expect(screen.getByText(/Failed to load movies/i)).toBeInTheDocument();
  });
});
```

## 🚀 Running the Application

### Backend API
```bash
cd src/api
dotnet run
```

### Frontend UI
```bash
cd src/ui
npm start
```

### Running Tests
```bash
# Backend tests
cd src/api/api.tests
dotnet test

# Frontend tests
cd src/ui
npm test
```

## 📋 Key Features

- **Resilient HTTP Calls**: Uses Polly for retry policies and timeout handling
- **Memory Caching**: Implements caching for both movie lists and details
- **Price Comparison**: Aggregates prices from multiple providers and finds cheapest
- **Error Handling**: Global exception filter with proper HTTP status codes
- **Concurrent Processing**: Uses SemaphoreSlim for controlled concurrency
- **JSON Deserialization**: Custom converter for decimal string handling
- **React Frontend**: Modern React app with routing and responsive design
- **Comprehensive Testing**: Unit and integration tests for both backend and frontend
