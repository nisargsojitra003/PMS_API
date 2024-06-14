using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PMS_API_DAL.Models;

[Table("product")]
public partial class Product
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? ModifiedAt { get; set; }

    public int CategoryId { get; set; }

    [StringLength(50)]
    public string? Categorytag { get; set; }

    [StringLength(250)]
    public string? Filename { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public int? Price { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? DeletedAt { get; set; }

    public int? UserId { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Products")]
    public virtual AspNetUser? User { get; set; }
}
