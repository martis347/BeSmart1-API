using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Authentication;
using Lunch.Authorization;

namespace Lunch.Host.Filters
{
    public class AuthorizationFilter: IActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        
        public AuthorizationFilter(IAuthorizationService authService)
        {
            _authorizationService = authService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers["access_token"];

            var success = _authorizationService.Authorize(token);
            if (!success)
            {
                throw new AuthenticationException("Provided access token is not valid.");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
