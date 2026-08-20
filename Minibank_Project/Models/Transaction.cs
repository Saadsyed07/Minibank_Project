using System;
using System.Collections.Generic;

namespace LoginForm.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }

    public virtual User User { get; set; } = null!;
}
