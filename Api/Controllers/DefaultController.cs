using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSisters.Dal;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("")]
    [AllowAnonymous]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("UrbanSisters by Gilles et Fransçois");
        }

    }
}