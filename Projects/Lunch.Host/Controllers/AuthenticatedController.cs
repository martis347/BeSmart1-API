using Lunch.Host.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Host.Controllers
{
    [ServiceFilter(typeof(AuthorizationFilter))]
    [ExceptionFilter]
    public class AuthenticatedController: Controller
    {
        
    }
}