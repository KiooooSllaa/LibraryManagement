using System;
using System.Collections.Generic;

namespace LibraryAuthApi.Models;

public partial class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public int RoleId { get; set; }


    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BorrowRecord> BorrowRecordLibrarians { get; set; } = new List<BorrowRecord>();

    public virtual ICollection<BorrowRecord> BorrowRecordUsers { get; set; } = new List<BorrowRecord>();


    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual Role Role { get; set; } = null!;
}
