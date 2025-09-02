using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

public class ExpenseFunctionTests
{
    private readonly Mock<ICosmosDbService> _mockCosmosDbService;
    private readonly ExpenseFunction _expenseFunction;

    public ExpenseFunctionTests()
    {
        _mockCosmosDbService = new Mock<ICosmosDbService>();
        _expenseFunction = new ExpenseFunction(_mockCosmosDbService.Object);
    }

    [Fact]
    public async Task CreateExpense_ReturnsCreatedResult_WhenExpenseIsValid()
    {
        // Arrange
        var expense = new Expense { Id = "1", Description = "Test Expense", Amount = 100, Date = DateTime.UtcNow };
        _mockCosmosDbService.Setup(service => service.CreateExpenseAsync(expense)).ReturnsAsync(expense);

        var httpRequest = new DefaultHttpContext().Request;
        httpRequest.Method = HttpMethods.Post;

        // Act
        var result = await _expenseFunction.CreateExpense(httpRequest, expense);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(expense, createdResult.Value);
    }

    [Fact]
    public async Task CreateExpense_ReturnsBadRequest_WhenExpenseIsInvalid()
    {
        // Arrange
        var httpRequest = new DefaultHttpContext().Request;
        httpRequest.Method = HttpMethods.Post;

        // Act
        var result = await _expenseFunction.CreateExpense(httpRequest, null);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    // Additional tests for other methods (Read, Update, Delete) can be added here
}