using System.ComponentModel.DataAnnotations;

namespace PMS_API_DAL.Models.CustomeModel
{
    public class CategoryDTO
    {
        public int? Id {  get; set; }
        public int? UserId { get; set; }

        [Required]
        //[StringLength(25, ErrorMessage = "Only 25 Characaters are Accepted")]
        //[RegularExpression(@"^[a-zA-Z]+( [a-zA-Z]*)*$", ErrorMessage = "Name must start with an alphabetic character and contain one or more words with spaces between them.")]
        public string Name { get; set; } = null!;

        //[StringLength(5)]
        [Required]
        //[RegularExpression(@"^\d{1,5}$", ErrorMessage = "The field must be between 1 and 5 digits.")]
        public string? Code { get; set; }

        //[StringLength(250, ErrorMessage = "Only 250 characters are accepted")]
        [Required]
        //[RegularExpression(@"^[a-zA-Z][\s\S]*$", ErrorMessage = "Description must start with an alphabet.")]
        public string? Description { get; set; }
    }
}
