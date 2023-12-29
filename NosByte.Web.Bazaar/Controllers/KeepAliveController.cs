using Microsoft.AspNetCore.Mvc;

namespace NosByte.Web.Bazaar.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class KeepAliveController
    {
        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            return new OkResult();
        }
    }
}
