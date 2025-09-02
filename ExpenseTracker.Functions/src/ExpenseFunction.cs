using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using ExpenseTracker.Functions.Models;

public class ExpenseFunction
{
    private readonly ICosmosDbService _cosmosDbService;

    public ExpenseFunction(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    [FunctionName("GetExpenses")]
    public async Task<IActionResult> GetExpenses(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "expenses")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Getting all expenses.");
        var expenses = await _cosmosDbService.GetAllExpensesAsync();
        return new OkObjectResult(expenses);
    }

    [FunctionName("CreateExpense")]
    public async Task<IActionResult> CreateExpense(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "expenses")] HttpRequest req,
        Expense expense)
    {
        if (expense == null)
        {
            return new BadRequestResult();
        }

        var createdExpense = await _cosmosDbService.CreateExpenseAsync(expense);
        return new CreatedResult($"/expenses/{createdExpense.Id}", createdExpense);
    }

    [FunctionName("UpdateExpense")]
    public async Task<IActionResult> UpdateExpense(
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
    public async Task<IActionResult> DeleteExpense(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "expenses/{id}")] HttpRequest req,
        string id,
        ILogger log)
    {
        log.LogInformation($"Deleting expense with ID: {id}");
        await _cosmosDbService.DeleteExpenseAsync(id);
        return new NoContentResult();
    }
}