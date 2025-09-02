using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ExpenseTracker.Functions.Models;
using Microsoft.Azure.Cosmos;
using Azure.Identity; // Add this

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    private const string DatabaseId = "ExpenseTrackerDb";
    private const string ContainerId = "Expenses";
    private const string AccountEndpoint = "https://expensetrackerdb.documents.azure.com:443/"; // e.g. https://your-account.documents.azure.com/

    public CosmosDbService()
    {
        // Use DefaultAzureCredential for MSI authentication
        var credential = new DefaultAzureCredential();
        _cosmosClient = new CosmosClient(AccountEndpoint, credential);
        _container = _cosmosClient.GetContainer(DatabaseId, ContainerId);
    }

    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        expense.Id = Guid.NewGuid().ToString();
        await _container.CreateItemAsync(expense, new PartitionKey(expense.Id));
        return expense;
    }

    public async Task<Expense> GetExpenseAsync(string id)
    {
        try
        {
            ItemResponse<Expense> response = await _container.ReadItemAsync<Expense>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<Expense>(query);
        List<Expense> expenses = new List<Expense>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            expenses.AddRange(response.Resource);
        }

        return expenses;
    }

    public async Task<Expense> UpdateExpenseAsync(Expense expense)
    {
        await _container.UpsertItemAsync(expense, new PartitionKey(expense.Id));
        return expense;
    }

    public async Task DeleteExpenseAsync(string id)
    {
        await _container.DeleteItemAsync<Expense>(id, new PartitionKey(id));
    }

    public async Task<IEnumerable<Expense>> SearchExpensesAsync(string searchTerm)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE CONTAINS(c.Description, @searchTerm) OR CONTAINS(c.Type, @searchTerm)")
            .WithParameter("@searchTerm", searchTerm);

        var iterator = _container.GetItemQueryIterator<Expense>(query);
        List<Expense> results = new List<Expense>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.Resource);
        }

        return results;
    }
}