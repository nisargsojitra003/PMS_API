using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models.CustomeModel;

namespace PMS_API.Controllers
{
    [ApiController]
    [Route("product")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProduct _ProductService;

        public ProductController(ILogger<ProductController> logger,IProduct product)
        {
            _logger = logger;
            _ProductService = product;
        }

        [Authorize]
        [HttpGet("getallproducts", Name = "GetProductsList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedList<AddProduct>>> GetAllProducts([FromQuery] SearchFilter searchFilter)
        {
            int totalProducts = await _ProductService.TotalProducts(searchFilter) ;
            string pageNumber = searchFilter.productPageNumber ?? "1";
            string pageSize = searchFilter.productPageSize ?? "5";
            PagedList<AddProduct> getAllProducts = await _ProductService.ProductList(int.Parse(pageNumber),int.Parse(pageSize),searchFilter);
            var response = new { 
                TotalProducts= totalProducts,
                productList = getAllProducts
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPost("create", Name = "CreateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateProduct([FromForm]AddProductDTO product)
        {
            if (product.ProductName == null)
            {
                return NotFound();
            }
            if(await _ProductService.CheckProductInDb(product.ProductName, (int)product.CategoryId,(int)product.userId))
            {
                return BadRequest();
            }
            await _ProductService.AddProductInDb(product);
            return Ok();
        }

        [Authorize]
        [HttpGet("getaddcategorylist", Name = "GetAddCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetAddCategories(int id)
        {
            AddProduct categories = await _ProductService.AddProductView(id);
            if (categories == null)
            {
                return BadRequest();
            }
            return Ok(categories);
        }

        [Authorize]
        [HttpGet("geteditcategorylist", Name = "GetEditCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetEditCategories()
        {
            EditProduct categories = await _ProductService.EditProductView();
            if (categories == null)
            {
                return BadRequest();
            }
            return Ok(categories);
        }

        [Authorize]
        [HttpGet("getproduct/{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EditProduct>> GetProduct(int id,int userId)
        {
            EditProduct product = await _ProductService.GetProduct(id, userId);
            if (product == null)
            {
                return BadRequest(); 
            }
            return Ok(product);
        }

        [Authorize]
        [HttpPut("update/{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int id , [FromForm] EditProductDTO editProduct)
        {
            if (id != editProduct.ProductId)
            {
                return NotFound();
            }
            
            if (await _ProductService.CheckProductEditName(id, editProduct))
            {
                return BadRequest();
            }
            
            await _ProductService.EditProduct(id, editProduct);
            return Ok();
        }

        [Authorize]
        [HttpPost("delete/{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                if(id == 0)
                {
                    return BadRequest();
                }
                await _ProductService.DeleteProduct(id);
                return Ok();
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost("deleteimage/{id:int}", Name = "DeleteProductImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            await _ProductService.DeleteProductImage(id);
            return Ok();
        }


    }
}