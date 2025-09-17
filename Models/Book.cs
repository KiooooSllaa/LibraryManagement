using System;
using System.Collections.Generic;

namespace LibraryAuthApi.Models;

public partial class Book
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int? AuthorId { get; set; }

    public int? PublishedYear { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Author? Author { get; set; }

    public virtual BookInventory? BookInventory { get; set; }

    public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

    public virtual Category? Category { get; set; }
}
