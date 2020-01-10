using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly JwtIssuerOptions _jwtOptions;

        public InscriptionController(UrbanSisterContext context, IMapper mapper, IOptions<JwtIssuerOptions> jwtOptions)
        {
            this._context = context;
            this._mapper = mapper;
            this._jwtOptions = jwtOptions.Value;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Dto.JwtToken), StatusCodes.Status201Created)]
        public async Task<IActionResult> Add([FromBody] Dto.UserInscription userInscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.User.FirstOrDefault(u => u.Email.Equals(userInscription.Email.ToLower())) != null)
            {
                return Conflict(ConflictErrorType.EmailAlreadyUsed);
            }

            User user = _mapper.Map<User>(userInscription);
            user.Password = new PasswordHasher<User>().HashPassword(user, user.Password);
            user.Email = user.Email.ToLower();

            var result = await _context.AddAsync(user);

            await _context.SaveChangesAsync();
            
            return Created("api/user/" + result.Entity.Id, await Utils.CreateTokenFor(user, _jwtOptions));
        }
    }
}