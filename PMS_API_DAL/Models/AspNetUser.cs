using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PMS_API_DAL.Models;

public partial class AspNetUser
{
    [Key]
    public int Id { get; set; }

    [StringLength(60)]
    public string Email { get; set; } = null!;

    [StringLength(50)]
    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [StringLength(20)]
    public string Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("User")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
