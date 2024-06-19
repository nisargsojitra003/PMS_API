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
        public CategoryController(ICategory category)
        {
            _CategoryService = category;
        }

        [Authorize]
        [HttpGet("getallcategories", Name = "GetCategoryList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PagedList<Category>>> GetAllCategories([FromQuery] SearchFilter searchFilter)
        {
            int totalCount = await _CategoryService.totalCount(searchFilter);
            string pageNumber = searchFilter.categoryPageNumber ?? "1";
            string pageSize = searchFilter.categoryPageSize ?? "5";
            PagedList<Category> allCategoties = await _CategoryService.CategoryList(int.Parse(pageNumber),int.Parse(pageSize),searchFilter);
            var response = new {
                TotalRecords = totalCount,
                categoriesList = allCategoties
            };

            return Ok(response);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("getcategory/{id:int}", Name = "GetCategory")]

        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            Category category = await _CategoryService.GetCategoryById(id);
            if (category == null)
            {
                return BadRequest();
            }
            return Ok(category);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("create", Name = "CreateCategory")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateCategory([FromForm] CategoryDTO addCategory)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            if (await _CategoryService.CheckCategoryNameInDb(addCategory.Name,(int)addCategory.UserId))
            {
                return BadRequest();
            }
            if (await _CategoryService.CheckCategoryCodeInDb(addCategory.Code, (int)addCategory.UserId))
            {
                return BadRequest();
            }
            await _CategoryService.AddCategoryInDb(addCategory);
            return Ok();
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("edit/{id:int}", Name = "EditCategory")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCategory(int id , [FromForm]CategoryDTO category)
        {
            if(id != category.Id)
            {
                return NotFound();
            }
            if (await _CategoryService.IsCategoryNameOrCodeExist(category, id,(int)category.UserId))
            {
                return BadRequest();
            }
            await _CategoryService.EditProduct(id, category);
            return Ok();
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("delete/{id:int}",Name ="DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (! await _CategoryService.CategoryCount(id))
            {
                return BadRequest();
            }
            await _CategoryService.DeleteCategory(id);
            return Ok();
        }

    }
}
