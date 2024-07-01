using PMS_API_DAL.Models.CustomeModel;
using PMS_API_DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace PMS_API_BAL.Interfaces
{
    public interface IActivity
    {
        public Task<PagedList<UserActivity>> UserActivityList(int pageNumber, int pageSize, SearchFilter searchFilter);
        public Task<int> TotalActivities(int userId);
    }
}