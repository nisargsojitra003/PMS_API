using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS_API_DAL.Models;

public partial class AspNetUser
{
    [Key]
    public int Id { get; set; }

    [StringLength(60)]
    public string Email { get; set; } = null!;

    [StringLength(300)]
    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [StringLength(20)]
    public string Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("User")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("User")]
    public virtual ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();
}
