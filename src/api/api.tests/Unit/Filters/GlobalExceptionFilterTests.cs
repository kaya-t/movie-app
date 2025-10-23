using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using MovieApp.Api.Exceptions;
using MovieApp.Api.Filters;
using FluentAssertions;

namespace MovieApp.Api.Tests.Unit.Filters
{
    public class GlobalExceptionFilterTests
    {
        private readonly Mock<ILogger<GlobalExceptionFilter>> _mockLogger;
        private readonly GlobalExceptionFilter _filter;
        private readonly ExceptionContext _exceptionContext;

        public GlobalExceptionFilterTests()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionFilter>>();
            _filter = new GlobalExceptionFilter(_mockLogger.Object);

            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            _exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void OnException_ShouldReturn404_WhenMovieNotFoundException()
        {
            // Arrange
            var exception = new MovieNotFoundException("Test Movie");
            _exceptionContext.Exception = exception;

            // Act
            _filter.OnException(_exceptionContext);

            // Assert
            _exceptionContext.ExceptionHandled.Should().BeTrue();
            var result = _exceptionContext.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public void OnException_ShouldReturn503_WhenProviderException()
        {
            // Arrange
            var exception = new ProviderException("test-provider", "Test error");
            _exceptionContext.Exception = exception;

            // Act
            _filter.OnException(_exceptionContext);

            // Assert
            _exceptionContext.ExceptionHandled.Should().BeTrue();
            var result = _exceptionContext.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(503);
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public void OnException_ShouldReturn500_WhenGenericException()
        {
            // Arrange
            var exception = new InvalidOperationException("Generic error");
            _exceptionContext.Exception = exception;

            // Act
            _filter.OnException(_exceptionContext);

            // Assert
            _exceptionContext.ExceptionHandled.Should().BeTrue();
            var result = _exceptionContext.Result as ObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public void OnException_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new MovieNotFoundException("Test Movie");
            _exceptionContext.Exception = exception;

            // Act
            _filter.OnException(_exceptionContext);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
