using System;
using System.Collections.Generic;

namespace api.DTOs;

public partial class StatusDTO
{
    public int Id { get; set; }
    public string StatusName { get; set; } = null!;
}
