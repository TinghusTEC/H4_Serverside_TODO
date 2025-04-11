using System;
using System.Collections.Generic;

namespace TodoApp.Data;

public partial class TodoList
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Item { get; set; }

    public virtual Cpr? User { get; set; }
}