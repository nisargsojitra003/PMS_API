using Microsoft.EntityFrameworkCore;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.DataContext;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Services
{
    public class ActivityServices : IActivity
    {
        private readonly ApplicationDbContext dbcontext;
        public ActivityServices(ApplicationDbContext context)
        {
            dbcontext = context;
        }

        public async Task<PagedList<UserActivity>> UserActivityList(int pageNumber, int pageSize, SearchFilter searchFilter)
        {
            IQueryable<UserActivity> activityList = dbcontext.UserActivities
                .Where(u => u.UserId == searchFilter.userId)
                .OrderByDescending(u => u.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchFilter.searchActivity))
            {
                activityList = activityList.Where(c => c.Description.ToLower().Trim().Contains(searchFilter.searchActivity.ToLower()));
                pageNumber = 1;
            }

            if (!string.IsNullOrEmpty(searchFilter.createdAtText))
            {
                activityList = activityList.Where(c=> c.CreatedAt.ToString().ToLower().Trim().Contains(searchFilter.createdAtText.ToLower().Trim()) &&
                (string.IsNullOrEmpty(searchFilter.searchActivity) || c.Description.ToLower().Trim().Contains(searchFilter.searchActivity.ToLower())));
            }

            if (activityList.Count() >= 2 && searchFilter.sortTypeActivity != 0)
            {
                activityList = (ActivitySortType)searchFilter.sortTypeActivity switch
                {
                    ActivitySortType.CreatedAtAsc => activityList.OrderBy(c => c.CreatedAt),
                    ActivitySortType.CreatedAtDesc => activityList.OrderByDescending(c => c.CreatedAt),
                    ActivitySortType.DescriptionAsc => activityList.OrderBy(c => c.Description),
                    ActivitySortType.DescriptionDesc => activityList.OrderByDescending(c => c.Description),
                    _ => activityList.AsQueryable(),
                };
            }

            int totalCount = activityList.Count();

            List<UserActivity> activityMainList = await activityList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new UserActivity
                {
                    Id = p.Id,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                }).ToListAsync();

            return new PagedList<UserActivity>(activityMainList, totalCount, pageNumber, pageSize);
        }

        public async Task<int> TotalActivitiesCounts(SearchFilter searchFilter)
        {
            return await dbcontext.UserActivities.Where(u => u.UserId == searchFilter.userId).CountAsync();
        }
    }
}