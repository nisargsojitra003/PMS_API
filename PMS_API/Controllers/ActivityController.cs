using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("activity")]
    public class ActivityController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IActivity _ActivityService;
            
        public ActivityController(ILogger<ProductController> logger, IActivity activity)
        {
            _logger = logger;
            _ActivityService = activity;
        }
        #region UserActivity
        /// <summary>
        /// Get all user's Activity.
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <returns>all activity of logged in user</returns>
        [Authorize]
        [HttpGet("getallactivity", Name = "GetAllActivityOfuser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("multipart/form-data")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "impactlevel", "pii" })]
        public async Task<ActionResult<PagedList<UserActivity>>> GetAllActivityOfuser([FromQuery] SearchFilter searchFilter)
        {
            if (searchFilter.userId <= 0)
            {
                return BadRequest();
            }
            int totalActivities = await _ActivityService.TotalActivities((int)searchFilter.userId);
            string pageNumber = searchFilter.activityPageNumber ?? "1";
            string pageSize = searchFilter.activityPageSize ?? "5";
            PagedList<UserActivity> activityList = await _ActivityService.UserActivityList(int.Parse(pageNumber), int.Parse(pageSize), searchFilter);
            ActivityListResponse activityListResponse = new ActivityListResponse()
            {
                TotalRecords = totalActivities,
                ActivityList = activityList
            };
            return Ok(activityListResponse);
        }
        #endregion
    }
}