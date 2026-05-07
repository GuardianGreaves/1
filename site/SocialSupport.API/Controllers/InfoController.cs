using Microsoft.AspNetCore.Mvc;
using SocialSupport.API.Models;

namespace SocialSupport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoController : ControllerBase
    {
        [HttpGet]
        public ActionResult<InfoModel> GetInfo()
        {
            var info = new InfoModel
            {
                Title = "АИС учета мер социальной поддержки",
                Description = "Система предназначена для автоматизации учета граждан пожилого возраста."
            };

            return Ok(info);
        }
    }
}