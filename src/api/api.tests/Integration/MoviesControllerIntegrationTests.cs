using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MovieApp.Api.Domain;
using MovieApp.Api.Providers;
using MovieApp.Api.Tests.Utilities;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace MovieApp.Api.Tests.Integration
{
    public class MoviesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MoviesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetMovies_ShouldReturnOk_WithMockedProviders()
        {
            // Arrange
            var mockProvider1 = new Mock<IMovieProviderClient>();
            var mockProvider2 = new Mock<IMovieProviderClient>();

            mockProvider1.Setup(x => x.Name).Returns("cinemaworld");
            mockProvider2.Setup(x => x.Name).Returns("filmworld");

            var movies1 = TestDataBuilder.GetCinemaworldMovies().Take(2).ToList();
            var movies2 = TestDataBuilder.GetFilmworldMovies().Take(2).ToList();

            mockProvider1.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(movies1);
            mockProvider2.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(movies2);

            var movie1Detail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0076759", 123.5m);
            var movie2Detail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0080684", 125.0m);
            var movie3Detail = TestDataBuilder.CreateFilmworldStarWarsDetail("fw0076759", 29.5m);
            var movie4Detail = TestDataBuilder.CreateFilmworldStarWarsDetail("fw0080684", 31.0m);

            mockProvider1.Setup(x => x.GetMovieAsync("cw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie1Detail);
            mockProvider1.Setup(x => x.GetMovieAsync("cw0080684", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie2Detail);
            mockProvider2.Setup(x => x.GetMovieAsync("fw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie3Detail);
            mockProvider2.Setup(x => x.GetMovieAsync("fw0080684", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movie4Detail);

            var customFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d => d.ServiceType == typeof(IMovieProviderClient)).ToList();
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IMovieProviderClient>(mockProvider1.Object);
                    services.AddSingleton<IMovieProviderClient>(mockProvider2.Object);
                });
            });

            var customClient = customFactory.CreateClient();

            // Act
            var response = await customClient.GetAsync("/api/movies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MoviesResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Movies.Should().HaveCount(2);
            result.Movies.Should().Contain(m => m.Title == "Star Wars: Episode IV - A New Hope" && m.CheapestPrice == 29.5m);
            result.Movies.Should().Contain(m => m.Title == "Star Wars: Episode V - The Empire Strikes Back" && m.CheapestPrice == 31.0m);
        }

        [Fact]
        public async Task GetMovie_ShouldReturnOk_WhenMovieExists()
        {
            // Arrange
            var mockProvider1 = new Mock<IMovieProviderClient>();
            mockProvider1.Setup(x => x.Name).Returns("cinemaworld");
            
            var mockProvider2 = new Mock<IMovieProviderClient>();
            mockProvider2.Setup(x => x.Name).Returns("filmworld");

            var cinemaworldMovies = TestDataBuilder.GetCinemaworldMovies().Take(1).ToList();
            mockProvider1.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cinemaworldMovies);
            
            var filmworldMovies = TestDataBuilder.GetFilmworldMovies().Take(1).ToList();
            mockProvider2.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(filmworldMovies);

            var movieDetail = TestDataBuilder.CreateCinemaworldStarWarsDetail("cw0076759", 123.5m);
            mockProvider1.Setup(x => x.GetMovieAsync("cw0076759", It.IsAny<CancellationToken>()))
                .ReturnsAsync(movieDetail);

            var customFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d => d.ServiceType == typeof(IMovieProviderClient)).ToList();
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IMovieProviderClient>(mockProvider1.Object);
                    services.AddSingleton<IMovieProviderClient>(mockProvider2.Object);
                });
            });

            var customClient = customFactory.CreateClient();

            // Act
            var response = await customClient.GetAsync("/api/movies/cw0076759");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MovieDetailResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Title.Should().Be("Star Wars: Episode IV - A New Hope");
            result.CheapestPrice.Should().Be(123.5m);
        }

        [Fact]
        public async Task GetMovie_ShouldReturnNotFound_WhenMovieDoesNotExist()
        {
            // Arrange
            var mockProvider = new Mock<IMovieProviderClient>();
            mockProvider.Setup(x => x.Name).Returns("cinemaworld");
            mockProvider.Setup(x => x.GetMovieAsync("nonexistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((MovieDetail?)null);
            mockProvider.Setup(x => x.GetMoviesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MovieSummary>());

            var customFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d => d.ServiceType == typeof(IMovieProviderClient)).ToList();
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IMovieProviderClient>(mockProvider.Object);
                });
            });

            var customClient = customFactory.CreateClient();

            // Act
            var response = await customClient.GetAsync("/api/movies/nonexistent");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMovies_ShouldReturnEmptyList_WhenNoProvidersAvailable()
        {
            // Arrange
            var customFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d => d.ServiceType == typeof(IMovieProviderClient)).ToList();
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }
                });
            });

            var customClient = customFactory.CreateClient();

            // Act
            var response = await customClient.GetAsync("/api/movies");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MoviesResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Movies.Should().BeEmpty();
        }
    }
}
