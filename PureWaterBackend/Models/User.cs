using System;
using System.Collections.Generic;

namespace PureWaterBackend.Models;

public partial class User
{
    public int Id { get; set; }

    public string GoogleId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public bool? IsDeleted { get; set; }

    public int DailyNormMl { get; set; }

    public virtual ICollection<Water> Water { get; set; } = new List<Water>();
}
