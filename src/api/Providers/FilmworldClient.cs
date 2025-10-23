using Microsoft.Extensions.Options;
using System.Text.Json;
using MovieApp.Api.Domain;
using MovieApp.Api.Configuration;
using MovieApp.Api.Exceptions;

namespace MovieApp.Api.Providers
{
    public class FilmworldClient : IMovieProviderClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
        private readonly ProviderOptions _opts;
        public string Name => "filmworld";

        public FilmworldClient(HttpClient http, IOptions<ProviderOptions> opts)
        {
            _http = http;
            _opts = opts.Value;
            _http.Timeout = TimeSpan.FromSeconds(30);

            var token = Environment.GetEnvironmentVariable("WEBJET_API_TOKEN") ?? _opts.ApiToken;
            if (string.IsNullOrWhiteSpace(token))
                throw new ProviderException("filmworld", "API token missing");

            _http.DefaultRequestHeaders.Add("x-access-token", token);
        }

        public async Task<List<MovieSummary>> GetMoviesAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync($"{_opts.Filmworld.BaseUrl}/movies", ct);
            if (!resp.IsSuccessStatusCode)
                throw new ProviderException(Name, $"Failed to get movies list. Status: {(int)resp.StatusCode}");

            try
            {
                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                var root = await JsonSerializer.DeserializeAsync<MovieListResponse>(stream, _json, ct);
                return root?.Movies ?? new();
            }
            catch (JsonException jx)
            {
                throw new ProviderException(Name, "Invalid JSON returned by provider", jx);
            }
        }

        public async Task<MovieDetail?> GetMovieAsync(string id, CancellationToken ct)
        {
            try
            {
                var resp = await _http.GetAsync($"{_opts.Filmworld.BaseUrl}/movie/{id}", ct);
                if (!resp.IsSuccessStatusCode) return null;
                var stream = await resp.Content.ReadAsStreamAsync(ct);
                return await JsonSerializer.DeserializeAsync<MovieDetail>(stream, _json, ct);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ProviderException(Name, $"Request timeout for movie {id}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ProviderException(Name, $"Request canceled for movie {id}", ex);
            }
        }
    }
}
