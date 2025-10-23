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