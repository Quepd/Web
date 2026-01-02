using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public partial class ProductCategory
{

    [Key]
    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Product? Product { get; set; }
}
