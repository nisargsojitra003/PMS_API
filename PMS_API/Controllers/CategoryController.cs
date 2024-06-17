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

        [HttpGet("getallcategories", Name = "GetCategoryList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            IEnumerable<Category> allCategoties = await _CategoryService.CategoryList();
            return Ok(allCategoties);
        }

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

        [HttpPost("create", Name = "CreateCategory")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateCategory([FromForm] CategoryDTO addCategory)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            if (await _CategoryService.CheckCategoryNameInDb(addCategory.Name))
            {
                return BadRequest();
            }
            if (await _CategoryService.CheckCategoryCodeInDb(addCategory.Code))
            {
                return BadRequest();
            }
            await _CategoryService.AddCategoryInDb(addCategory);
            return Ok();
        }

        [HttpPut("edit/{id:int}", Name = "EditCategory")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCategory(int id , [FromForm]CategoryDTO category)
        {
            if(id != category.Id)
            {
                return NotFound();
            }
            if (await _CategoryService.IsCategoryNameOrCodeExist(category, id))
            {
                return BadRequest();
            }
            await _CategoryService.EditProduct(id, category);
            return Ok();
        }

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
