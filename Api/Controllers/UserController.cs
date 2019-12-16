using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly IMapper _mapper;

        public UserController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        // GET: /user
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.User.ToArray().Select(user => _mapper.Map<Dto.User>(user)));
        }

        [HttpPost]
        public IActionResult inscription([FromBody] Dto.UserInscription userInscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(_context.User.Where(user => user.Email.Equals(userInscription.Email.ToLower())).FirstOrDefault() != null)
            {
                return Conflict();
            }

            User user = _mapper.Map<User>(userInscription);
            user.Password = new PasswordHasher<User>().HashPassword(user, user.Password);
            user.Email = user.Email.ToLower();

            var result = _context.Add(user);

            _context.SaveChanges();

            return Created("api/user/" + result.Entity.Id, _mapper.Map<Dto.User>(result.Entity));
        }
    }
}
