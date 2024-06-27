using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IProduct _ProductService;
        public DashboardController(IProduct product)
        {
            _ProductService = product;
        }

        #region GetCategoryandproduct Counts
        /// <summary>
        /// Get Counts of user's TotalCategory and TotalProduct
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("dashboardinfo", Name = "UserDashboardInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DashboardData>> UserDashboardInfo(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            DashboardData counts = await _ProductService.DashboardData(id);
            return Ok(counts);
        }
        #endregion
    }
}