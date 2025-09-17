using System;
using System.Collections.Generic;

namespace LibraryAuthApi.Models;

public partial class BookInventory
{
    public int BookId { get; set; }

    public int? TotalCopies { get; set; }

    public int? AvailableCopies { get; set; }

    public string? Title { get; set; }

    public virtual Book Book { get; set; } = null!;
}
