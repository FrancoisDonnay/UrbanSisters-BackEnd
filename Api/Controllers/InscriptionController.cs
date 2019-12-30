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
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> add([FromBody] Dto.UserInscription userInscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(_context.User.FirstOrDefault(user => user.Email.Equals(userInscription.Email.ToLower())) != null)
            {
                return Conflict(ConflictErrorType.EmailAlreadyUsed);
            }

            User user = _mapper.Map<User>(userInscription);
            user.Password = new PasswordHasher<User>().HashPassword(user, user.Password);
            user.Email = user.Email.ToLower();

            var result = await _context.AddAsync(user);

            await _context.SaveChangesAsync();
            
            IEnumerable<Claim> claims = new List<Claim>(new []
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64)
            });
            
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
                expire_at = ((DateTimeOffset)_jwtOptions.Expiration).ToUnixTimeSeconds()
            };

            return Created("api/user/" + result.Entity.Id, response);
        }

        private static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }
    }
}