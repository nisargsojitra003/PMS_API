using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;
using PMS_API_DAL.Models.CustomeModel;
using PMS_API_DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace PMS_API_BAL.Services
{
    public class LoginService : ILogin
    {
        private readonly ApplicationDbContext dbcontext;
        private readonly ICategory _CategoryService;
        private readonly ActivityMessages activityMessages;
        public LoginService(ApplicationDbContext context, ICategory categoryService, ActivityMessages _activityMessages)
        {
            dbcontext = context;
            _CategoryService = categoryService;
            activityMessages = _activityMessages;
        }
        public async Task<bool> ValidateUserCredentials(UserInfo userInfo)
        {
            AspNetUser? user = await dbcontext.AspNetUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == userInfo.Email.ToLower()) ?? null;

            if (user != null)
            {
                return GetHashPassword(user.Password, userInfo.Password);
            }

            return false;
        }

        public async Task<AspNetUser> LoggedInUserInfo(UserInfo userInfo)
        {
            AspNetUser? user = await dbcontext.AspNetUsers.FirstOrDefaultAsync(a => a.Email == userInfo.Email) ?? null;
            return user;
        }

        public async Task<bool> CheckIfEmailExist(string email)
        {
            return await dbcontext.AspNetUsers.AnyAsync(i => i.Email == email);
        }

        public async Task CreateNewUser(UserInfo createUser)
        {
            string hashedPassword = SetHashPassword(createUser.Password);

            AspNetUser aspNetUser = new AspNetUser()
            {
                Email = createUser.Email.Trim(),
                Password = hashedPassword,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
            };

            await dbcontext.AspNetUsers.AddAsync(aspNetUser);
            await dbcontext.SaveChangesAsync();

            //Add entry in useractivity When account created successfully.
            string description = activityMessages.createAccountRecord.Replace("{1}", createUser.Email);
            await _CategoryService.CreateActivity(description, aspNetUser.Id);
        }

        public static string SetHashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(32);
            byte[] hashBytes = new byte[48];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }

        public static bool GetHashPassword(string savedHashedPassword, string passwordToCheck)
        {
            byte[] hashBytes = Convert.FromBase64String(savedHashedPassword);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(passwordToCheck, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
