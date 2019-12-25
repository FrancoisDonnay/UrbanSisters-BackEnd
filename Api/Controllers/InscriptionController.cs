using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class InscriptionController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;

        public InscriptionController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult add([FromBody] Dto.UserInscription userInscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(_context.User.FirstOrDefault(user => user.Email.Equals(userInscription.Email.ToLower())) != null)
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