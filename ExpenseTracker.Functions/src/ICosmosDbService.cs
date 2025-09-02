using ExpenseTracker.Functions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICosmosDbService
{
    Task<Expense> CreateExpenseAsync(Expense expense);
    Task<Expense> GetExpenseAsync(string id);
    Task<IEnumerable<Expense>> GetAllExpensesAsync();
    Task<Expense> UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(string id);
}