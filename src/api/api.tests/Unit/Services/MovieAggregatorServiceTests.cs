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
using FluentAssertions;

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
            _mockProvider2 = new Mock<IMovieProviderClient>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<MovieAggregatorService>>();
            _mockOptions = new Mock<IOptions<AggregatorOptions>>();

            _mockProvider1.Setup(x => x.Name).Returns("cinemaworld");
            _mockProvider2.Setup(x => x.Name).Returns("filmworld");

            _mockOptions.Setup(x => x.Value).Returns(TestDataBuilder.CreateAggregatorOptions());

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
