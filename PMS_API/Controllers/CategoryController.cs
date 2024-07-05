using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategory _CategoryService;
        private readonly ActivityMessages activityMessages;
        public CategoryController(ICategory category, ActivityMessages _activityMessages)
        {
            _CategoryService = category;
            activityMessages = _activityMessages;
        }

        #region GetAllCategories
        /// <summary>
        /// Get all category list.
        /// </summary>
        /// <param name="searchFilter">Filter criteria for searching categories.</param>
        /// <returns>All categories that match the filter and pagination criteria.</returns>
        [Authorize]
        [HttpGet("list", Name = "GetCategoryList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 15)]
        public async Task<ActionResult<PagedList<Category>>> List([FromQuery] SearchFilter searchFilter)
        {
            int totalCategoryCounts = await _CategoryService.TotalCategoriesCount(searchFilter);

            string pageNumber = searchFilter.categoryPageNumber ?? "1";
            string pageSize = searchFilter.categoryPageSize ?? "5";

            PagedList<Category> categoryList = await _CategoryService.CategoryList(int.Parse(pageNumber), int.Parse(pageSize), searchFilter);

            SharedListResponse<Category> categoryListResponse = new SharedListResponse<Category>
            {
                TotalRecords = totalCategoryCounts,
                List = categoryList
            };

            return Ok(categoryListResponse);
        }
        #endregion

        #region GetCategoryById
        /// <summary>
        /// Gets the details of a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to retrieve.</param>
        /// <returns>The details of the specified category.</returns>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("get/{id:int}", Name = "GetCategory")]
        public async Task<ActionResult<Category>> Get(int id, int userId)
        {
            try
            {
                if (!await _CategoryService.CheckIfCategoryExists(id) || id <= 0)
                {
                    return NotFound();
                }

                if (!await _CategoryService.GetCategoryTypeById(id, userId))
                {
                    if (!await _CategoryService.CheckUsersCategory(id, userId))
                    {
                        return NotFound();
                    }
                }

                Category category = await _CategoryService.GetCategoryById(id);

                if (category == null)
                {
                    return NotFound("Category not found.");
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get category: {ex.Message}");
            }
        }
        #endregion

        #region CategoryPostmethod
        /// <summary>
        /// Create category method.
        /// </summary>
        /// <param name="addCategory"></param>
        /// <returns>Indicating success or failure</returns>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("create", Name = "CreateCategory")]
        public async Task<ActionResult> Create([FromForm] CategoryDTO category)
        {
            if (await _CategoryService.CheckCategoryIfAlreadyExist(category))
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            if (category.Id == 0)
            {
                await _CategoryService.Create(category);
            }
            else
            {
                await _CategoryService.Update(category);
            }

            string description = category.Id == 0 ? activityMessages.add : activityMessages.edit;
            await _CategoryService.CreateActivity(description.Replace("{1}", category.Name).Replace("{2}", nameof(EntityNameEnum.category)), (int)category.UserId);
           
            return Ok();
        }
        #endregion

        #region DeleteCategory
        /// <summary>
        /// Soft deletes a particular category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>ActionResult indicating success or failure of the operation.</returns>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("delete/{id:int}", Name = "DeleteCategory")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _CategoryService.CheckIfCategoryExists(id) || await _CategoryService.CategoryCount(id))
            {
                return BadRequest();
            }

            await _CategoryService.DeleteCategory(id);

            string categoryName = await _CategoryService.CategoryName(id);
            int userid = await _CategoryService.CategoryUserid(id);

            if (categoryName == "" || userid == 0)
            {
                return BadRequest();
            }

            string description = activityMessages.delete.Replace("{1}", categoryName).Replace("{2}", nameof(EntityNameEnum.category));

            await _CategoryService.CreateActivity(description, userid);

            return Ok();
        }
        #endregion
    }
}