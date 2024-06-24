using System.ComponentModel.DataAnnotations;

namespace PMS_API_DAL.Models.CustomeModel
{
    public class CategoryDTO
    {
        public int? Id {  get; set; }
        public int? UserId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string? Code { get; set; }

        [Required]
        public string? Description { get; set; }
    }
}
