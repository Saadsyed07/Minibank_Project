using System;
using System.Collections.Generic;

namespace LoginForm.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Password { get; set; }

    public string? AccountNumber { get; set; }

    public decimal Balance { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
