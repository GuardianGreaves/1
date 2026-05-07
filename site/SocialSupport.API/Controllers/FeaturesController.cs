using Microsoft.AspNetCore.Mvc;

namespace SocialSupport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeaturesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<string>> GetFeatures()
        {
            var features = new List<string>
            {
                "Учет граждан",
                "Работа с мероприятиями",
                "Формирование отчетов",
                "Учет заявок"
            };

            return Ok(features);
        }
    }
}