using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface ILogin
    {
        public Task<bool> ValidateUserCredentials(UserInfo userInfo);
        public Task<AspNetUser> LoggedInUserInfo(UserInfo userInfo);
        public Task<bool> CheckIfEmailExist(string email);
        public Task CreateNewUser(UserInfo createUser);
    }
}
