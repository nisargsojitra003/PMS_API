using PMS_API_DAL.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_API_BAL.Interfaces
{
    public interface IJwt
    {
        public string GenerateToken(AspNetUser user);
        public bool ValidateToken(string token, out JwtSecurityToken jwtSecurityToken);
    }
}
