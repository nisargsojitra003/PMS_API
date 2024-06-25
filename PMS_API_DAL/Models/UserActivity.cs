using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS_API_DAL.Models;

[Table("UserActivity")]
public partial class UserActivity
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [StringLength(150)]
    public string Description { get; set; } = null!;

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserActivities")]
    public virtual AspNetUser User { get; set; } = null!;
}
