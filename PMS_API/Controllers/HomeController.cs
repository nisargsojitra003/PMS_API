using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("home")]
    public class HomeController : ControllerBase
    {
        private readonly IProduct _ProductService;
        public HomeController(IProduct product)
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
        [HttpGet("index", Name = "Index")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TotalCount>> Index(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            TotalCount counts = await _ProductService.totalCount(id);
            return Ok(counts);
        }
        #endregion
    }
}