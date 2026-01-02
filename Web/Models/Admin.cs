using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class Admin
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public virtual User IdNavigation { get; set; } = null!;
}
