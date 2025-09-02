using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json;

public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    private const string DatabaseId = "ExpenseTrackerDb";
    private const string ContainerId = "Expenses";

    public CosmosDbService()
    {
        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        var connectionString = $"AccountEndpoint=<your-cosmos-db-endpoint>;AccountKey=<your-cosmos-db-key>;";

        _cosmosClient = new CosmosClient(connectionString);
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
            expenses.AddRange(response);
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
}