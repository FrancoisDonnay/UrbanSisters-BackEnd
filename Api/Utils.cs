using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UrbanSisters.Model;

namespace UrbanSisters.Api
{
    public class Utils
    {
        public static async Task<Dto.JwtToken> CreateTokenFor(User user, JwtIssuerOptions jwtOptions)
        {
            IEnumerable<Claim> claims = new List<Claim>(new []
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, await jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, Utils.ToUnixEpochDate(jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64)
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
                issuer: jwtOptions.Issuer,
                audience: jwtOptions.Audience,
                claims: claims,
                notBefore: jwtOptions.NotBefore,
                expires: jwtOptions.Expiration,
                signingCredentials: jwtOptions.SigningCredentials
            );
            
            return new Dto.JwtToken{access_token = new JwtSecurityTokenHandler().WriteToken(token), expire_at = ((DateTimeOffset)jwtOptions.Expiration).ToUnixTimeSeconds()};
        }
        
        public static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }
    }
}