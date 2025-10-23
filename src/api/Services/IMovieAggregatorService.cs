using MovieApp.Api.Domain;

namespace MovieApp.Api.Services
{
    public interface IMovieAggregatorService
    {
        Task<MoviesResponse> GetMoviesAsync(CancellationToken cancellationToken);
        Task<MovieDetailResponse?> GetMovieAsync(string id, CancellationToken cancellationToken);
    }
}
