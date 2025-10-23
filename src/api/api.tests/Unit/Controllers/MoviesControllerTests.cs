using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MovieApp.Api.Controllers;
using MovieApp.Api.Domain;
using MovieApp.Api.Exceptions;
using MovieApp.Api.Providers;
using MovieApp.Api.Services;
using MovieApp.Api.Configuration;
using MovieApp.Api.Tests.Utilities;
using FluentAssertions;

namespace MovieApp.Api.Tests.Unit.Controllers
{
    public class MoviesControllerTests
    {
        private readonly Mock<IMovieAggregatorService> _mockService;
        private readonly MoviesController _controller;

        public MoviesControllerTests()
        {
            _mockService = new Mock<IMovieAggregatorService>();
            _controller = new MoviesController(_mockService.Object);
        }

        [Fact]
        public async Task GetMovies_ShouldReturnOkResult_WhenServiceReturnsData()
        {
            // Arrange
            var expectedResponse = new MoviesResponse
            {
                Movies = new List<MovieItemResponse>
                {
                    new MovieItemResponse
                    {
                        Id = "Star Wars: Episode IV - A New Hope",
                        Title = "Star Wars: Episode IV - A New Hope",
                        Year = "1977",
                        Type = "movie",
                        Poster = "https://m.media-amazon.com/images/M/MV5BOTIyMDY2NGQtOGJjNi00OTk4LWFhMDgtYmE3M2NiYzM0YTVmXkEyXkFqcGdeQXVyNTU1NTcwOTk@._V1_SX300.jpg",
                        CheapestPrice = 29.5m,
                        CheapestProvider = "filmworld",
                        PricesByProvider = new Dictionary<string, decimal> 
                        { 
                            ["cinemaworld"] = 123.5m, 
                            ["filmworld"] = 29.5m 
                        }
                    }
                }
            };

            _mockService.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetMovies(CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<MoviesResponse>>();
            var actionResult = result.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task GetMovie_ShouldReturnOkResult_WhenMovieExists()
        {
            // Arrange
            var movieId = "cw0076759";
            var movieDetail = new MovieDetailResponse
            {
                Id = movieId,
                Title = "Star Wars: Episode IV - A New Hope",
                Year = "1977",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BOTIyMDY2NGQtOGJjNi00OTk4LWFhMDgtYmE3M2NiYzM0YTVmXkEyXkFqcGdeQXVyNTU1NTcwOTk@._V1_SX300.jpg",
                CheapestPrice = 123.5m,
                CheapestProvider = "cinemaworld",
                PricesByProvider = new Dictionary<string, decimal> { ["cinemaworld"] = 123.5m }
            };

            _mockService.Setup(x => x.GetMovieAsync(movieId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(movieDetail);

            // Act
            var result = await _controller.GetMovie(movieId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<MovieDetailResponse>>();
            var actionResult = result.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.Value.Should().Be(movieDetail);
        }

        [Fact]
        public async Task GetMovie_ShouldThrowMovieNotFoundException_WhenMovieNotFound()
        {
            // Arrange
            var nonExistentId = "cw9999999";
            _mockService.Setup(x => x.GetMovieAsync(nonExistentId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MovieNotFoundException(nonExistentId));

            // Act
            await Assert.ThrowsAsync<MovieNotFoundException>(() => 
                _controller.GetMovie(nonExistentId, CancellationToken.None));
        }

        [Fact]
        public async Task GetMovie_ShouldReturnBadRequest_WhenIdIsEmpty()
        {
            // Act
            var result = await _controller.GetMovie("", CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<MovieDetailResponse>>();
            var actionResult = result.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
        }

        [Fact]
        public async Task GetMovie_ShouldReturnBadRequest_WhenIdIsWhitespace()
        {
            // Act
            var result = await _controller.GetMovie("   ", CancellationToken.None);

            // Assert
            result.Should().BeOfType<ActionResult<MovieDetailResponse>>();
            var actionResult = result.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
        }
    }
}
