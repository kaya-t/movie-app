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
            var exception = context.Exception;
            var result = exception switch
            {
                MovieNotFoundException => new ObjectResult(new { message = exception.Message })
                {
                    StatusCode = 404
                },
                ProviderException => new ObjectResult(new { message = exception.Message })
                {
                    StatusCode = 503
                },
                _ => new ObjectResult(new { message = "An unexpected error occurred" })
                {
                    StatusCode = 500
                }
            };

            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
