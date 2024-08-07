﻿using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API_BAL.Interfaces
{
    public interface IActivity
    {
        public Task<PagedList<UserActivity>> UserActivityList(int pageNumber, int pageSize, SearchFilter searchFilter);
        public Task<int> TotalActivitiesCounts(SearchFilter searchFilter);
    }
}