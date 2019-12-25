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
using UrbanSisters.Model;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginAsync([FromBody] Dto.Login loginModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            User user = _context.User.Include(u => u.Relookeuse).FirstOrDefault(person => person.Email.Equals(loginModel.Email.ToLower()));

            if (user == null || passwordHasher.VerifyHashedPassword(user, user.Password, loginModel.Password).Equals(PasswordVerificationResult.Failed))
            {
                return Unauthorized();
            }

            IEnumerable<Claim> claims = new List<Claim>(new []
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64)
            });
            
            if(user.IsAdmin)
            {
                claims = claims.Union(new[]
                {
                    new Claim(ClaimTypes.Role, "admin")
                });
            }

            if(user.Relookeuse != null)
            {
                claims = claims.Union(new[]
                {
                    new Claim(ClaimTypes.Role, "relookeuse")
                });
            }

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials
            );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
            };

            return Ok(response);
        }

        private static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }
    }
}
