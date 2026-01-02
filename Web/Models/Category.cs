using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public bool? Active { get; set; }
}
