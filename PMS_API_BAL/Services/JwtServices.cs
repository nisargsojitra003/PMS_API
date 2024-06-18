using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;
using PMS_API_DAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PMS_API_BAL.Services
{
    public class JwtServices : IJwt
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext dbcontext;
        public JwtServices(IConfiguration configuration, ApplicationDbContext applicationDbContext)
        {
            _configuration = configuration;
            dbcontext = applicationDbContext;
        }
        public string GenerateToken(AspNetUser user)
        {
            string list = "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Role, user.Role),
                new Claim("userId", user.Id.ToString()),
                new Claim("AccessRoleIDs", list)
            };


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token, out JwtSecurityToken jwtSecurityToken)
        {
            jwtSecurityToken = null;

            if (token == null)
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero

                }, out SecurityToken validatedToken);

                // Corrected access to the validatedToken
                jwtSecurityToken = (JwtSecurityToken)validatedToken;

                if (jwtSecurityToken != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
