using API.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.JwtTokens
{
    public class JwtProvider(IOptions<JwtOptions> options)
    {
        private readonly JwtOptions _options = options.Value;

        public string GenerateToken(User user)
        {
            Claim[] claims = [new("id_user", user.IdUser.ToString()), new("role", user.IdRoleNavigation.NameRole)];

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddHours(_options.ExpitesHours));

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }
    }
}
