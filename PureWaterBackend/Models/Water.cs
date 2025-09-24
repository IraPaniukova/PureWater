using System;
using System.Collections.Generic;

namespace PureWaterBackend.Models;

public partial class Water
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public int AmountMl { get; set; }

    public virtual User User { get; set; } = null!;
}
