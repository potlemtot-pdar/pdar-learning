using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ExpenseTracker.Tests
{

    public class ExpenseFunctionTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbServiceMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly ExpenseFunction _function;

        public ExpenseFunctionTests()
        {
            _cosmosDbServiceMock = new Mock<ICosmosDbService>();
            _loggerMock = new Mock<ILogger>();
            _function = new ExpenseFunction(_cosmosDbServiceMock.Object);
        }

        [Fact]
        public async Task GetExpenses_ReturnsOkObjectResult_WithExpenses()
        {
            var expenses = new List<Expense>
        {
            new Expense { Id = "1", Description = "Lunch", Amount = 10, Date = DateTime.UtcNow, Type = ExpenseType.Food }
        };
            _cosmosDbServiceMock.Setup(s => s.GetAllExpensesAsync()).ReturnsAsync(expenses);

            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext);

            var result = await _function.GetExpenses(req, _loggerMock.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expenses, okResult.Value);
        }

        [Fact]
        public async Task CreateExpense_ReturnsCreatedResult_WhenExpenseIsValid()
        {
            var expense = new Expense { Description = "Dinner", Amount = 20, Date = DateTime.UtcNow, Type = ExpenseType.Food };
            var createdExpense = new Expense { Id = "2", Description = "Dinner", Amount = 20, Date = expense.Date, Type = ExpenseType.Food };
            _cosmosDbServiceMock.Setup(s => s.CreateExpenseAsync(expense)).ReturnsAsync(createdExpense);

            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext);

            var result = await _function.CreateExpense(req, expense);

            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal($"/expenses/{createdExpense.Id}", createdResult.Location);
            Assert.Equal(createdExpense, createdResult.Value);
        }

        [Fact]
        public async Task CreateExpense_ReturnsBadRequestResult_WhenExpenseIsNull()
        {
            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext);

            var result = await _function.CreateExpense(req, null);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsOkObjectResult_WhenExpenseIsValid()
        {
            var expense = new Expense { Id = "3", Description = "Taxi", Amount = 15, Date = DateTime.UtcNow, Type = ExpenseType.Travel };
            var expenseJson = System.Text.Json.JsonSerializer.Serialize(expense);

            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext)
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(expenseJson))
            };

            _cosmosDbServiceMock.Setup(s => s.UpdateExpenseAsync(expense)).ReturnsAsync(expense);

            var result = await _function.UpdateExpense(req, "3", _loggerMock.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedExpense = Assert.IsType<Expense>(okResult.Value);
            Assert.Equal(expense.Id, returnedExpense.Id);
            Assert.Equal(expense.Description, returnedExpense.Description);
            Assert.Equal(expense.Amount, returnedExpense.Amount);
            Assert.Equal(expense.Date, returnedExpense.Date);
            Assert.Equal(expense.Type, returnedExpense.Type);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsBadRequestObjectResult_WhenExpenseIsNull()
        {
            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext)
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes("null"))
            };

            var result = await _function.UpdateExpense(req, "4", _loggerMock.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid expense data.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsBadRequestObjectResult_WhenExpenseIdMismatch()
        {
            var expense = new Expense { Id = "wrong", Description = "Taxi", Amount = 15, Date = DateTime.UtcNow, Type = ExpenseType.Travel };
            var expenseJson = System.Text.Json.JsonSerializer.Serialize(expense);

            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext)
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(expenseJson))
            };

            var result = await _function.UpdateExpense(req, "expected", _loggerMock.Object);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid expense data.", badRequest.Value);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsNoContentResult()
        {
            var httpContext = new DefaultHttpContext();
            var req = new DefaultHttpRequest(httpContext);

            _cosmosDbServiceMock.Setup(s => s.DeleteExpenseAsync("5")).Returns(Task.CompletedTask);

            var result = await _function.DeleteExpense(req, "5", _loggerMock.Object);

            Assert.IsType<NoContentResult>(result);
        }
    }
}