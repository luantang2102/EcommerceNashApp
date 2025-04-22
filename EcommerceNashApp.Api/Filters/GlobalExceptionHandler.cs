using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Filters
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            return exception switch
            {
                ValidationException validationException => await HandleValidationExceptionAsync(httpContext, validationException, cancellationToken),
                AppException appException => await HandleAppExceptionAsync(httpContext, appException, cancellationToken),
                AccessDeniedException accessDeniedException => await HandleAccessDeniedExceptionAsync(httpContext, accessDeniedException, cancellationToken),
                _ => await HandleGenericExceptionAsync(httpContext, exception, cancellationToken)
            };
        }

        private async Task<bool> HandleAppExceptionAsync(HttpContext httpContext, AppException exception, CancellationToken cancellationToken)
        {
            var errorCode = exception.GetErrorCode();
            var attributes = exception.GetAttributes();

            var response = attributes.Count > 0
                ? new ApiResponse<Dictionary<string, object>>(errorCode.GetCode(), errorCode.GetMessage(), attributes)
                : new ApiResponse<Dictionary<string, object>>(errorCode.GetCode(), errorCode.GetMessage());

            httpContext.Response.StatusCode = errorCode.GetStatus();
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
        private async Task<bool> HandleValidationExceptionAsync(HttpContext httpContext, ValidationException exception, CancellationToken cancellationToken)
        {
            var errorCode = exception.GetErrorCode();
            var response = new ApiResponse<Dictionary<string, object>>(
                errorCode.GetCode(),
                errorCode.GetMessage(),
                exception.GetAttributes());

            httpContext.Response.StatusCode = errorCode.GetStatus();
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        private async Task<bool> HandleAccessDeniedExceptionAsync(HttpContext httpContext, AccessDeniedException exception, CancellationToken cancellationToken)
        {
            var errorCode = exception.GetErrorCode();

            var response = new ApiResponse<object>(
                errorCode.GetCode(),
                errorCode.GetMessage()
            );

            httpContext.Response.StatusCode = errorCode.GetStatus();
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        private async Task<bool> HandleGenericExceptionAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = "An unexpected error occurred",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
