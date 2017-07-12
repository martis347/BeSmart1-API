using System.Threading.Tasks;
using Lunch.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Lunch.Host.Controllers
{
    [Route("api/[controller]")]
    public class FeedbackController : Controller
    {
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(ILogger<FeedbackController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public void SelectDishes([FromBody] FeedbackRating feedback)
        {
            _logger.LogCritical(JsonConvert.SerializeObject(feedback));
        }
    }
}