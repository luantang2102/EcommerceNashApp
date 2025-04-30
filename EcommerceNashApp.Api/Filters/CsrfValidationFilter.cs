using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcommerceNashApp.Api.Filters
{
    public class CsrfValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is SkipCsrfValidationAttribute))
            {
                return;
            }

            if (context.HttpContext.Request.Method != "GET")
            {
                var csrfToken = context.HttpContext.Request.Headers["X-CSRF-Token"];
                var cookieCsrfToken = context.HttpContext.Request.Cookies["csrf"];
                if (string.IsNullOrEmpty(csrfToken) || csrfToken != cookieCsrfToken)
                {
                    throw new AccessDeniedException(ErrorCode.INVALID_CSRF_TOKEN);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}