using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ExpenseTracker.Functions.Models;
using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;

namespace ExpenseTracker.Tests
{
    public class CosmosDbServiceTests
    {
        private readonly Mock<Container> _containerMock;
        private readonly Mock<CosmosClient> _cosmosClientMock;
        private readonly CosmosDbService _service;

        public CosmosDbServiceTests()
        {
            _containerMock = new Mock<Container>();
            _cosmosClientMock = new Mock<CosmosClient>();
            _cosmosClientMock
                .Setup(c => c.GetContainer(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_containerMock.Object);

            // Use reflection to inject mocks since CosmosDbService has no DI constructor
            _service = (CosmosDbService)Activator.CreateInstance(typeof(CosmosDbService), true);
            typeof(CosmosDbService)
                .GetField("_cosmosClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_service, _cosmosClientMock.Object);
            typeof(CosmosDbService)
                .GetField("_container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_service, _containerMock.Object);
        }

        [Fact]
        public async Task CreateExpenseAsync_ShouldCreateAndReturnExpense()
        {
            var expense = new Expense { Description = "Lunch", Amount = 10, Date = DateTime.UtcNow, Type = ExpenseType.Food };
            _containerMock
                .Setup(c => c.CreateItemAsync(expense, It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ItemResponse<Expense>>());

            var result = await _service.CreateExpenseAsync(expense);

            Assert.NotNull(result.Id);
            Assert.Equal(expense.Description, result.Description);
        }

        [Fact]
        public async Task GetExpenseAsync_ShouldReturnExpense_WhenFound()
        {
            var expense = new Expense { Id = "1", Description = "Dinner", Amount = 20, Date = DateTime.UtcNow, Type = ExpenseType.Food };
            var responseMock = new Mock<ItemResponse<Expense>>();
            responseMock.Setup(r => r.Resource).Returns(expense);

            _containerMock
                .Setup(c => c.ReadItemAsync<Expense>("1", It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMock.Object);

            var result = await _service.GetExpenseAsync("1");

            Assert.Equal("1", result.Id);
            Assert.Equal("Dinner", result.Description);
        }

        [Fact]
        public async Task GetExpenseAsync_ShouldReturnNull_WhenNotFound()
        {
            _containerMock
                .Setup(c => c.ReadItemAsync<Expense>("2", It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0));

            var result = await _service.GetExpenseAsync("2");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllExpensesAsync_ShouldReturnAllExpenses()
        {
            var expenses = new List<Expense>
        {
            new Expense { Id = "1", Description = "A", Amount = 1, Date = DateTime.UtcNow, Type = ExpenseType.Food },
            new Expense { Id = "2", Description = "B", Amount = 2, Date = DateTime.UtcNow, Type = ExpenseType.Travel }
        };

            var iteratorMock = new Mock<FeedIterator<Expense>>();
            iteratorMock.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);

            var feedResponseMock = new Mock<FeedResponse<Expense>>();
            feedResponseMock.Setup(r => r.Resource).Returns(expenses);

            iteratorMock
                .Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedResponseMock.Object);

            _containerMock
                .Setup(c => c.GetItemQueryIterator<Expense>(It.IsAny<QueryDefinition>(), null, null))
                .Returns(iteratorMock.Object);

            var result = await _service.GetAllExpensesAsync();

            Assert.Equal(2, ((List<Expense>)result).Count);
        }

        [Fact]
        public async Task UpdateExpenseAsync_ShouldUpsertAndReturnExpense()
        {
            var expense = new Expense { Id = "1", Description = "Updated", Amount = 30, Date = DateTime.UtcNow, Type = ExpenseType.Utilities };
            _containerMock
                .Setup(c => c.UpsertItemAsync(expense, It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ItemResponse<Expense>>());

            var result = await _service.UpdateExpenseAsync(expense);

            Assert.Equal("1", result.Id);
            Assert.Equal("Updated", result.Description);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ShouldDeleteExpense()
        {
            _containerMock
                .Setup(c => c.DeleteItemAsync<Expense>("1", It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ItemResponse<Expense>>());

            await _service.DeleteExpenseAsync("1");

            _containerMock.Verify(c => c.DeleteItemAsync<Expense>("1", It.IsAny<PartitionKey>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchExpensesAsync_ShouldReturnMatchingExpenses()
        {
            var expenses = new List<Expense>
        {
            new Expense { Id = "1", Description = "Taxi", Amount = 15, Date = DateTime.UtcNow, Type = ExpenseType.Travel }
        };

            var iteratorMock = new Mock<FeedIterator<Expense>>();
            iteratorMock.SetupSequence(i => i.HasMoreResults)
                .Returns(true)
                .Returns(false);

            var feedResponseMock = new Mock<FeedResponse<Expense>>();
            feedResponseMock.Setup(r => r.Resource).Returns(expenses);

            iteratorMock
                .Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedResponseMock.Object);

            _containerMock
                .Setup(c => c.GetItemQueryIterator<Expense>(It.IsAny<QueryDefinition>(), null, null))
                .Returns(iteratorMock.Object);

            var result = await _service.SearchExpensesAsync("Taxi");

            Assert.Single(result);
            Assert.Equal("Taxi", ((List<Expense>)result)[0].Description);
        }
    }
}