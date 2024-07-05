using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models.CustomeModel;
using System.Reflection.Metadata.Ecma335;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("product")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProduct _ProductService;
        private readonly ICategory _CategoryService;
        private readonly ActivityMessages activityMessages;

        public ProductController(ILogger<ProductController> logger, IProduct product, ICategory categoryService, ActivityMessages _activityMessages)
        {
            _logger = logger;
            _ProductService = product;
            _CategoryService = categoryService;
            activityMessages = _activityMessages;
        }

        #region GetAllProducts
        /// <summary>
        /// Get all product list
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <returns>All products that match the filter and pagination criteria.</returns>
        [Authorize]
        [HttpGet("list", Name = "GetProductsList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(Duration = 15)]
        public async Task<ActionResult<PagedList<AddProduct>>> List([FromQuery] SearchFilter searchFilter)
        {
            int totalProductsCounts = await _ProductService.TotalProductsCounts(searchFilter);

            string pageNumber = searchFilter.productPageNumber ?? "1";
            string pageSize = searchFilter.productPageSize ?? "5";

            PagedList<AddProduct> productList = await _ProductService.ProductList(int.Parse(pageNumber), int.Parse(pageSize), searchFilter);

            SharedListResponse<AddProduct> productListResponse = new SharedListResponse<AddProduct>()
            {
                TotalRecords = totalProductsCounts,
                List = productList
            };

            return Ok(productListResponse);
        }
        #endregion



        #region CreateProduct Post Method
        /// <summary>
        /// Create Product and Edit Method in Combine
        /// </summary>
        /// <param name="product"></param>
        /// <returns>indicating success or failure</returns>
        [Authorize]
        [HttpPost("create", Name = "CreateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Create([FromForm] AddProductDTO product)
        {
            if (product.ProductName == null)
            {
                return NotFound();
            }

            if (await _ProductService.CheckProductIfExists(product))
            {
                return BadRequest();
            }

            if (product.ProductId == 0)
            {
                await _ProductService.AddProductInDb(product);
            }
            else
            {
                await _ProductService.EditProduct((int)product.ProductId, product);
            }

            string description = product.ProductId == 0 ? activityMessages.add : activityMessages.edit;
            await _CategoryService.CreateActivity(description.Replace("{1}", product.ProductName).Replace("{2}", nameof(EntityNameEnum.product)), (int)product.userId);
            
            return Ok();
        }
        #endregion

        #region Category List by user for add method
        /// <summary>
        /// Get all category list for custom models
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("getaddcategorylist", Name = "GetAddCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetAddCategories(int id)
        {
            AddProduct categories = await _ProductService.AddProductViewCategories(id);

            if (categories == null)
            {
                return BadRequest();
            }

            return Ok(categories);
        }
        #endregion

        #region Get Product for edit method
        /// <summary>
        /// Get Product by product id 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns>all product's details</returns>
        [Authorize]
        [HttpGet("get/{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EditProduct>> Get(int id, int userId)
        {
            try
            {
                if (!await _ProductService.CheckProduct(id) || !await _ProductService.CheckUsersProduct(id, userId))
                {
                    return NotFound();
                }

                EditProduct product = await _ProductService.GetProduct(id, userId);
                if (product == null)
                {
                    return BadRequest();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to delete product: {ex.Message}");
            }
        }
        #endregion

        #region DeleteProduct
        /// <summary>
        /// Soft deletes a particular product.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>ActionResult indicating success or failure of the operation.</returns>
        [Authorize]
        [HttpPost("delete/{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (!await _ProductService.CheckProduct(id) || id <= 0)
                {
                    return BadRequest();
                }

                if (!await _ProductService.DeleteProduct(id))
                {
                    return BadRequest();
                }

                string productName = await _ProductService.ProductName(id);
                int userId = await _ProductService.ProductUserid(id);

                if (productName == string.Empty || userId <= 0)
                {
                    return BadRequest();
                }

                string description = activityMessages.delete.Replace("{1}", productName).Replace("{2}", nameof(EntityNameEnum.product));

                await _CategoryService.CreateActivity(description, (int)userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to delete product: {ex.Message}");
            }
        }
        #endregion

        #region Delete Product's image
        /// <summary>
        /// Delete Product's Image by product id
        /// </summary>
        /// <param name="id">product id</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("deleteimage/{id:int}", Name = "DeleteProductImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            if (!await _ProductService.CheckProduct(id) || id <= 0)
            {
                return BadRequest();
            }

            await _ProductService.DeleteProductImage(id);

            return Ok();
        }
        #endregion
    }
}