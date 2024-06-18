using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;
using PMS_API_DAL.Models.CustomeModel;
using PMS_API_DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace PMS_API_BAL.Services
{
    public class LoginService : ILogin
    {
        private readonly ApplicationDbContext dbcontext;
        public LoginService(ApplicationDbContext context)
        {
            dbcontext = context;
        }

        public async Task<AspNetUser> LoginUser(Login login)
        {
            AspNetUser? user = await dbcontext.AspNetUsers.FirstOrDefaultAsync(u =>
                                u.Email.ToLower() == login.Email.ToLower() &&
                                u.Password == login.Password);
            return user;
        }

        public async Task<bool> CheckEmailInDb(string email)
        {
            var user = await dbcontext.AspNetUsers.FirstOrDefaultAsync(i => i.Email == email);
            if (user == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task CreateNewUser(Login createUser)
        {
            AspNetUser aspNetUser = new AspNetUser()
            {
                Email = createUser.Email.Trim(),
                Password = createUser.Password.Trim(),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
            };
            await dbcontext.AspNetUsers.AddAsync(aspNetUser);
            await dbcontext.SaveChangesAsync();
        }

    }
}
