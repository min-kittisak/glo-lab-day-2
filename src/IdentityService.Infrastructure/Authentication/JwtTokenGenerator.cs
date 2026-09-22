using IdentityService.Application.Features.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityService.Infrastructure.Authentication
{
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _options;

        public JwtTokenGenerator(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken(
            Guid userId, 
            string username, 
            string displayName, 
            string emailAddress, 
            string? departmentCode)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, username),
                new(JwtRegisteredClaimNames.Email, emailAddress),
                new("display_name", displayName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };


            if (!string.IsNullOrWhiteSpace(departmentCode))
            {
                claims.Add(
                    new Claim(
                        "department_code",
                        departmentCode));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _options.ExpireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);


        }
    }
}
