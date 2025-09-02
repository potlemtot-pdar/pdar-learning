using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

public static class ExpenseFunction
{
    private static readonly CosmosDbService _cosmosDbService = new CosmosDbService();

    [FunctionName("GetExpenses")]
    public static async Task<IActionResult> GetExpenses(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "expenses")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Getting all expenses.");
        var expenses = await _cosmosDbService.GetExpensesAsync();
        return new OkObjectResult(expenses);
    }

    [FunctionName("CreateExpense")]
    public static async Task<IActionResult> CreateExpense(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "expenses")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Creating a new expense.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var expense = JsonConvert.DeserializeObject<Expense>(requestBody);

        if (expense == null)
        {
            return new BadRequestObjectResult("Invalid expense data.");
        }

        await _cosmosDbService.CreateExpenseAsync(expense);
        return new CreatedResult($"/expenses/{expense.Id}", expense);
    }

    [FunctionName("UpdateExpense")]
    public static async Task<IActionResult> UpdateExpense(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "expenses/{id}")] HttpRequest req,
        string id,
        ILogger log)
    {
        log.LogInformation($"Updating expense with ID: {id}");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var expense = JsonConvert.DeserializeObject<Expense>(requestBody);

        if (expense == null || expense.Id != id)
        {
            return new BadRequestObjectResult("Invalid expense data.");
        }

        await _cosmosDbService.UpdateExpenseAsync(expense);
        return new OkObjectResult(expense);
    }

    [FunctionName("DeleteExpense")]
    public static async Task<IActionResult> DeleteExpense(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "expenses/{id}")] HttpRequest req,
        string id,
        ILogger log)
    {
        log.LogInformation($"Deleting expense with ID: {id}");
        await _cosmosDbService.DeleteExpenseAsync(id);
        return new NoContentResult();
    }
}