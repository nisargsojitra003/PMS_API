using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PMS_API_DAL.Models;

[Table("category")]
public partial class Category
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedAt { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? UserId { get; set; }

    public bool IsSystem { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [ForeignKey("UserId")]
    [InverseProperty("Categories")]
    public virtual AspNetUser? User { get; set; }
}
