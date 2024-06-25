using PMS_API_DAL.Models;
using System.IdentityModel.Tokens.Jwt;

namespace PMS_API_BAL.Interfaces
{
    public interface IJwt
    {
        public string GenerateToken(AspNetUser user);
        public bool ValidateToken(string token, out JwtSecurityToken jwtSecurityToken);
    }
}
