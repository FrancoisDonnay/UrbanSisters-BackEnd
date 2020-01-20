using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UrbanSisters.Dal;
using User = UrbanSisters.Model.User;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly JwtIssuerOptions _jwtOptions;

        public LoginController(UrbanSisterContext context, IOptions<JwtIssuerOptions> jwtOptions)
        {
            this._context = context;
            this._jwtOptions = jwtOptions.Value;
        }

        // POST: /login
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Dto.JwtToken), StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginAsync([FromBody] Dto.Login loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            User user = await _context.User.Include(u => u.Relookeuse).FirstOrDefaultAsync(person => person.Email.Equals(loginModel.Email.ToLower()));

            if (user == null || passwordHasher.VerifyHashedPassword(user, user.Password, loginModel.Password).Equals(PasswordVerificationResult.Failed))
            {
                return Unauthorized();
            }
            
            return Ok(await Utils.CreateTokenFor(user, _jwtOptions));
        }
    }
}
