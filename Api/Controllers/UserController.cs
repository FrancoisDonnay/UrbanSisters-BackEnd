using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly UrbanSisterContext _context;

        public UserController(UrbanSisterContext context)
        {
            this._context = context;
        }
        // GET: /user
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.User.ToArray());
        }


        [HttpPost]
        public IActionResult inscription([FromBody] Dto.UserInscriptionModel userInscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var config = new MapperConfiguration(cfg => cfg.CreateMap<Dto.UserInscriptionModel, User>());
            _context.Add(config.CreateMapper().Map<User>(userInscription));
            _context.SaveChanges();

            return Ok();
        }
    }
}
