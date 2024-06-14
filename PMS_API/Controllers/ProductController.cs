using Microsoft.AspNetCore.Mvc;
using PMS_API_BAL.Interfaces;
using PMS_API_DAL.Models;
using PMS_API_DAL.Models.CustomeModel;
using System.Net;

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

        [HttpGet("getallproducts", Name = "GetProductsList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AddProduct>>> GetAllProducts()
        {
            IEnumerable<AddProduct> getAllProducts = await _ProductService.ProductList();
            return Ok(getAllProducts);
        }

        [HttpPost("create", Name = "CreateProduct")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateProduct([FromForm]AddProductDTO product)
        {
            if (product.ProductName == null)
            {
                return NotFound();
            }
            if(await _ProductService.CheckProductInDb(product.ProductName, (int)product.CategoryId))
            {
                return BadRequest();
            }
            await _ProductService.AddProductInDb(product);
            return Ok();
        }

        [HttpGet("getaddcategorylist", Name = "GetAddCategories")]
        public async Task<ActionResult> GetAddCategories()
        {
            AddProduct categories = await _ProductService.AddProductView();
            if (categories == null)
            {
                return BadRequest();
            }
            return Ok(categories);
        }

        [HttpGet("geteditcategorylist", Name = "GetEditCategories")]
        public async Task<ActionResult> GetEditCategories()
        {
            EditProduct categories = await _ProductService.EditProductView();
            if (categories == null)
            {
                return BadRequest();
            }
            return Ok(categories);
        }

        [HttpGet("getproduct/{id:int}", Name = "GetProduct")]
        public async Task<ActionResult<EditProduct>> GetProduct(int id)
        {
            EditProduct product = await _ProductService.GetProduct(id);
            if (product == null)
            {
                return BadRequest(); 
            }
            return Ok(product);
        }

        [HttpPut("update/{id:int}", Name = "UpdateProduct")]
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

        [HttpPost("delete/{id:int}", Name = "DeleteProduct")]
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



    }
}