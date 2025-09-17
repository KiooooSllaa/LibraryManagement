using System;
using System.Collections.Generic;

namespace LibraryAuthApi.Models;

public partial class PasswordResetToken
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Token { get; set; }

    public DateTime? Expiration { get; set; }

    public bool? IsUsed { get; set; }

    public virtual User? User { get; set; }
}
