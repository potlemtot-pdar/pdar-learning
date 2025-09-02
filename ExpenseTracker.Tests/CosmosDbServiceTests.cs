using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ExpenseTracker.Tests
{
    public class CosmosDbServiceTests
    {
        private readonly Mock<CosmosClient> _mockCosmosClient;
        private readonly Mock<Container> _mockContainer;
        private readonly CosmosDbService _cosmosDbService;

        public CosmosDbServiceTests()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _cosmosDbService = new CosmosDbService(_mockCosmosClient.Object, "databaseId", "containerId");
        }

        [Fact]
        public async Task CreateExpense_ShouldReturnExpense_WhenSuccessful()
        {
            var expense = new Expense { Id = "1", Description = "Test Expense", Amount = 100, Date = DateTime.UtcNow };
            _mockContainer.Setup(c => c.CreateItemAsync(expense, null, null, default))
                .ReturnsAsync(new ItemResponse<Expense>(expense, null, 0, null, null));

            var result = await _cosmosDbService.CreateExpense(expense);

            Assert.Equal(expense.Id, result.Id);
            Assert.Equal(expense.Description, result.Description);
        }

        [Fact]
        public async Task GetExpense_ShouldReturnExpense_WhenExists()
        {
            var expenseId = "1";
            var expense = new Expense { Id = expenseId, Description = "Test Expense", Amount = 100, Date = DateTime.UtcNow };
            _mockContainer.Setup(c => c.ReadItemAsync<Expense>(expenseId, new PartitionKey(expenseId), null, default))
                .ReturnsAsync(new ItemResponse<Expense>(expense, null, 0, null, null));

            var result = await _cosmosDbService.GetExpense(expenseId);

            Assert.Equal(expenseId, result.Id);
            Assert.Equal(expense.Description, result.Description);
        }

        [Fact]
        public async Task DeleteExpense_ShouldReturnTrue_WhenSuccessful()
        {
            var expenseId = "1";
            _mockContainer.Setup(c => c.DeleteItemAsync<Expense>(expenseId, new PartitionKey(expenseId), null, default))
                .ReturnsAsync(new ItemResponse<Expense>(null, null, 0, null, null));

            var result = await _cosmosDbService.DeleteExpense(expenseId);

            Assert.True(result);
        }

        [Fact]
        public async Task GetAllExpenses_ShouldReturnListOfExpenses()
        {
            var expenses = new List<Expense>
            {
                new Expense { Id = "1", Description = "Test Expense 1", Amount = 100, Date = DateTime.UtcNow },
                new Expense { Id = "2", Description = "Test Expense 2", Amount = 200, Date = DateTime.UtcNow }
            };

            var mockFeedIterator = new Mock<FeedIterator<Expense>>();
            mockFeedIterator.SetupSequence(m => m.HasMoreResults)
                .Returns(true)
                .Returns(false);
            mockFeedIterator.Setup(m => m.ReadNextAsync(default))
                .ReturnsAsync(new FeedResponse<Expense>(expenses, null, null, null, null));

            _mockContainer.Setup(c => c.GetItemQueryIterator<Expense>(It.IsAny<QueryDefinition>(), null, null))
                .Returns(mockFeedIterator.Object);

            var result = await _cosmosDbService.GetAllExpenses();

            Assert.Equal(2, result.Count);
        }
    }
}