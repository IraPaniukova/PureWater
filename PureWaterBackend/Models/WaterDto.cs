using System;
using System.Collections.Generic;

namespace PureWaterBackend.Models;

public partial class WaterDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public int AmountMl { get; set; }

}
