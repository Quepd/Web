using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class ProductImage
{
    public int ProductId { get; set; }

    public string? ImageSrc { get; set; }

    public virtual Product Product { get; set; } = null!;
}
