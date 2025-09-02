using System;

namespace ExpenseTracker.Functions.Models;

public class Expense
{
    public string Id { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public ExpenseType Type { get; set; }
}