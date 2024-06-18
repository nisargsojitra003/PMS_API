using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface ILogin
    {
        public Task<AspNetUser> LoginUser(Login model);
        public Task<bool> CheckEmailInDb(string email);
        public Task CreateNewUser(Login createUser);
    }
}
