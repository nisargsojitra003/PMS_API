using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PMS_API_DAL.Models.CustomeModel
{
    public class EditProduct
    {
        public List<CategoryList>? categories { get; set; }
        public List<Category>? CategoryList {  get; set; }
        public int ProductId { get; set; }

        [Required(ErrorMessage = "ProductName is required")]
        [StringLength(40, ErrorMessage = "Only 40 Characters are Accepted")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9 .]*$", ErrorMessage = "Product name must start with an alphabetic character and can contain alphanumeric characters, spaces, and floating numbers.")]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        public DateTime? ModiFiedDate { get; set; }

        [Required(ErrorMessage = "Categorytag is required")]
        public string? CategoryTag { get; set; }
        public string? CategoryName { get; set; }
        public string? FileName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(499, ErrorMessage = "Only 499 Characaters are Accepted")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Please Write a Description.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Product Price is required")]
        [Range(1, int.MaxValue, ErrorMessage = "The Price must be greater than zero")]
        [RegularExpression(@"^\d{1,8}(\.\d{1,2})?$", ErrorMessage = "The Price must be a number with up to 8 digits and up to 2 decimal places")]
        public int? Price { get; set; }
    }

    public class CategoryList
    {
        public int? Id {  get; set; }
        public string? Name { get; set; }
    }

}
