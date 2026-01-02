using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int Status { get; set; }

    public double Shipping { get; set; }

    public double Total { get; set; }

    public double Discount { get; set; }

    public double GrandTotal { get; set; }
	public string? Address { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
