using Microsoft.AspNetCore.Mvc;
using SocialSupport.API.Models;

namespace SocialSupport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        [HttpGet]
        public ActionResult<DownloadModel> GetDownloadInfo()
        {
            var data = new DownloadModel
            {
                FileName = "setup.exe",
                Version = "1.0",
                Requirements = "Windows 10/11"
            };

            return Ok(data);
        }
    }
}