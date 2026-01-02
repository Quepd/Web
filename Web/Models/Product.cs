using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class Product
{
    public int Id { get; set; }

    public string ProductId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int Type { get; set; }

    public double Price { get; set; }

    public double? Discount { get; set; }

    public string? Content { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ProductImage? ProductImage { get; set; }

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
