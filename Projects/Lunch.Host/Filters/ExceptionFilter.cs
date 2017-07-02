using Lunch.Domain.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Authentication;

namespace Lunch.Host.Filters
{
    public class ExceptionFilter: IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            ApiError error;
            if (context.Exception is AuthenticationException)
            {
                error = new ApiError(context.Exception);
                context.HttpContext.Response.StatusCode = 401;
            } 
            else if (context.Exception is ApiException)
            {
                ApiException ex = (ApiException) context.Exception;
                error = new ApiError(context.Exception);
                context.HttpContext.Response.StatusCode = ex.StatusCode;
            }
            else
            {
                #if DEBUG
                    error = new ApiError(context.Exception);
                #else
                    error = new ApiError("An unhandled exception has occured");
                #endif
                
                context.HttpContext.Response.StatusCode = 500;
            }
            
            context.Result = new JsonResult(error);
        }
    }
}