using BlazorWasm.Server.Services;
using BlazorWasm.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlazorWasm.Server.Tests.Services;

public class MockAITaskParsingServiceSimpleTests
{
    private readonly Mock<ILogger<MockAITaskParsingService>> _mockLogger;
    private readonly MockAITaskParsingService _service;

    public MockAITaskParsingServiceSimpleTests()
    {
        _mockLogger = new Mock<ILogger<MockAITaskParsingService>>();
        _service = new MockAITaskParsingService(_mockLogger.Object);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithEmptyInput_ReturnsFailure()
    {
        // Act
        var result = await _service.ParseNaturalLanguageAsync("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Input cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var input = "Create a new task for testing the parsing service functionality";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Title);
        Assert.Equal(Priority.Medium, result.Priority); // Default priority
        Assert.Equal(0.75, result.ConfidenceScore); // Mock confidence score
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithUrgentKeyword_ExtractsCriticalPriority()
    {
        // Arrange
        var input = "Urgent task that needs immediate attention and processing";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Priority.Critical, result.Priority);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithHighPriorityKeyword_ExtractsHighPriority()
    {
        // Arrange
        var input = "High priority task that requires significant attention today";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Priority.High, result.Priority);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithLowPriorityKeyword_ExtractsLowPriority()
    {
        // Arrange
        var input = "Low priority task that can be done when time permits";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Priority.Low, result.Priority);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithAssigneePattern_ExtractsAssignee()
    {
        // Arrange
        var input = "Review the quarterly report with Alice for analysis";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Alice", result.Assignee);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithTomorrowKeyword_ExtractsDueDate()
    {
        // Arrange
        var input = "Complete the project documentation tomorrow for review";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.DueDate);
        Assert.True(result.DueDate.Value.Date > DateTime.Now.Date);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_WithNextWeekKeyword_ExtractsDueDate()
    {
        // Arrange
        var input = "Deploy the application by next week for production use";

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.DueDate);
        Assert.True(result.DueDate.Value.Date > DateTime.Now.Date);
    }

    [Theory]
    [InlineData("Create user authentication system")]
    [InlineData("Fix bug in payment processing module")]
    [InlineData("Update API documentation for developers")]
    [InlineData("Refactor database connection logic for performance")]
    [InlineData("Implement email notification system with templates")]
    public async Task ParseNaturalLanguageAsync_WithVariousInputs_AlwaysSucceeds(string input)
    {
        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Title);
        Assert.NotNull(result.Priority);
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_HasProcessingDelay()
    {
        // Arrange
        var input = "Test task to verify processing delay simulation";
        var startTime = DateTime.Now;

        // Act
        var result = await _service.ParseNaturalLanguageAsync(input);
        var endTime = DateTime.Now;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True((endTime - startTime).TotalMilliseconds >= 400); // Should have ~500ms delay
    }
}
