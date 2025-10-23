using MovieApp.Api.Domain;

namespace MovieApp.Api.Providers
{
    public interface IMovieProviderClient
    {
        string Name { get; }
        Task<List<MovieSummary>> GetMoviesAsync(CancellationToken ct);
        Task<MovieDetail?> GetMovieAsync(string id, CancellationToken ct);
    }
}
