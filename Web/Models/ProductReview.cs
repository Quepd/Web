using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class ProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Title { get; set; } = null!;

    public int Rating { get; set; }

    public DateTime PublishedAt { get; set; }

    public string? Content { get; set; }

    public virtual Product Product { get; set; } = null!;
}
