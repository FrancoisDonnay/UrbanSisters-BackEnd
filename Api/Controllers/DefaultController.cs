using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("")]
    [AllowAnonymous]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Index()
        {
            return Ok("UrbanSisters by Gilles et François");
        }

    }
}