using BlazorWasm.Server.Services;
using BlazorWasm.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlazorWasm.Server.Tests.Services;

public class ParsingAccuracyTests
{
    private readonly Mock<ILogger<MockAITaskParsingService>> _mockLogger;
    private readonly MockAITaskParsingService _service;

    public ParsingAccuracyTests()
    {
        _mockLogger = new Mock<ILogger<MockAITaskParsingService>>();
        _service = new MockAITaskParsingService(_mockLogger.Object);
    }

    public class TestCase
    {
        public string Input { get; set; } = "";
        public string ExpectedTitle { get; set; } = "";
        public Priority ExpectedPriority { get; set; } = Priority.Medium;
        public string? ExpectedAssignee { get; set; }
        public bool ExpectedHasDueDate { get; set; }
        public string Description { get; set; } = "";
    }

    public static IEnumerable<object[]> AccuracyTestCases()
    {
        var testCases = new List<TestCase>
        {
            // Basic tasks
            new() { 
                Input = "Review Q3 report", 
                ExpectedTitle = "Review Q3 report", 
                ExpectedPriority = Priority.Medium,
                Description = "Basic task without modifiers"
            },
            new() { 
                Input = "Fix login bug", 
                ExpectedTitle = "Fix login bug", 
                ExpectedPriority = Priority.Medium,
                Description = "Simple bug fix task"
            },
            
            // Priority extraction
            new() { 
                Input = "Urgent: Fix production server", 
                ExpectedTitle = "Fix production server", 
                ExpectedPriority = Priority.High,
                Description = "Urgent priority task"
            },
            new() { 
                Input = "High priority database optimization", 
                ExpectedTitle = "database optimization", 
                ExpectedPriority = Priority.High,
                Description = "High priority task"
            },
            new() { 
                Input = "Low priority UI polish", 
                ExpectedTitle = "UI polish", 
                ExpectedPriority = Priority.Low,
                Description = "Low priority task"
            },
            new() { 
                Input = "Critical security patch ASAP", 
                ExpectedTitle = "security patch", 
                ExpectedPriority = Priority.High,
                Description = "Critical priority with ASAP"
            },
            
            // Assignee extraction
            new() { 
                Input = "Update documentation with Alice", 
                ExpectedTitle = "Update documentation", 
                ExpectedPriority = Priority.Medium,
                ExpectedAssignee = "Alice",
                Description = "Task with 'with' assignee pattern"
            },
            new() { 
                Input = "Fix bug assigned to John", 
                ExpectedTitle = "Fix bug", 
                ExpectedPriority = Priority.Medium,
                ExpectedAssignee = "John",
                Description = "Task with 'assigned to' pattern"
            },
            new() { 
                Input = "Code review for Sarah", 
                ExpectedTitle = "Code review for Sarah", 
                ExpectedPriority = Priority.Medium,
                ExpectedAssignee = "Sarah",
                Description = "Task with 'for' assignee pattern"
            },
            
            // Due date extraction
            new() { 
                Input = "Complete report by Monday", 
                ExpectedTitle = "Complete report", 
                ExpectedPriority = Priority.Medium,
                ExpectedHasDueDate = true,
                Description = "Task with specific day deadline"
            },
            new() { 
                Input = "Deploy changes by next week", 
                ExpectedTitle = "Deploy changes", 
                ExpectedPriority = Priority.Medium,
                ExpectedHasDueDate = true,
                Description = "Task with relative deadline"
            },
            new() { 
                Input = "Send report due tomorrow", 
                ExpectedTitle = "Send report", 
                ExpectedPriority = Priority.Medium,
                ExpectedHasDueDate = true,
                Description = "Task with tomorrow deadline"
            },
            
            // Complex combinations
            new() { 
                Input = "High priority: Review Q3 report with Alice by Friday", 
                ExpectedTitle = "Review Q3 report", 
                ExpectedPriority = Priority.High,
                ExpectedAssignee = "Alice",
                ExpectedHasDueDate = true,
                Description = "Complex task with all elements"
            },
            new() { 
                Input = "Urgent database backup assigned to Bob due tonight", 
                ExpectedTitle = "database backup", 
                ExpectedPriority = Priority.High,
                ExpectedAssignee = "Bob",
                ExpectedHasDueDate = true,
                Description = "Complex urgent task"
            },
            new() { 
                Input = "Low priority documentation update with team by next week", 
                ExpectedTitle = "documentation update", 
                ExpectedPriority = Priority.Low,
                ExpectedAssignee = "team",
                ExpectedHasDueDate = true,
                Description = "Complex low priority task"
            },
            
            // Edge cases
            new() { 
                Input = "meeting", 
                ExpectedTitle = "meeting", 
                ExpectedPriority = Priority.Medium,
                Description = "Single word task"
            },
            new() { 
                Input = "Fix the really important security vulnerability ASAP with the security team", 
                ExpectedTitle = "Fix the really important security vulnerability", 
                ExpectedPriority = Priority.High,
                ExpectedAssignee = "the security team",
                Description = "Long task with multiple keywords"
            },
            new() { 
                Input = "Update user interface by Monday with UI team", 
                ExpectedTitle = "Update user interface", 
                ExpectedPriority = Priority.Medium,
                ExpectedAssignee = "UI team",
                ExpectedHasDueDate = true,
                Description = "Task with both assignee and due date"
            }
        };

        return testCases.Select(tc => new object[] { tc });
    }

    [Theory]
    [MemberData(nameof(AccuracyTestCases))]
    public async Task ParseNaturalLanguageAsync_AccuracyTest(TestCase testCase)
    {
        // Act
        var result = await _service.ParseNaturalLanguageAsync(testCase.Input);

        // Assert
        Assert.True(result.IsSuccess, $"Parsing failed for: {testCase.Input}");
        
        // Title extraction accuracy
        Assert.Equal(testCase.ExpectedTitle, result.Title);
        
        // Priority extraction accuracy
        Assert.Equal(testCase.ExpectedPriority, result.Priority);
        
        // Assignee extraction accuracy
        if (testCase.ExpectedAssignee != null)
        {
            Assert.Equal(testCase.ExpectedAssignee, result.Assignee);
        }
        
        // Due date extraction accuracy
        if (testCase.ExpectedHasDueDate)
        {
            Assert.NotNull(result.DueDate);
        }
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_OverallAccuracyRate_MeetsThreshold()
    {
        // Arrange
        var testCases = AccuracyTestCases().Select(tc => (TestCase)tc[0]).ToList();
        var correctParses = 0;
        var totalTests = testCases.Count;
        const double requiredAccuracy = 0.90; // 90% accuracy threshold

        // Act
        foreach (var testCase in testCases)
        {
            try
            {
                var result = await _service.ParseNaturalLanguageAsync(testCase.Input);
                
                // Check if parsing meets expectations
                var titleCorrect = result.Title == testCase.ExpectedTitle;
                var priorityCorrect = result.Priority == testCase.ExpectedPriority;
                var assigneeCorrect = testCase.ExpectedAssignee == null || 
                                     result.Assignee == testCase.ExpectedAssignee;
                var dueDateCorrect = !testCase.ExpectedHasDueDate || 
                                    result.DueDate != null;

                if (titleCorrect && priorityCorrect && assigneeCorrect && dueDateCorrect)
                {
                    correctParses++;
                }
            }
            catch
            {
                // Parsing failure counts as incorrect
                continue;
            }
        }

        // Assert
        var accuracy = (double)correctParses / totalTests;
        Assert.True(accuracy >= requiredAccuracy, 
            $"Parsing accuracy {accuracy:P2} is below the required threshold of {requiredAccuracy:P2}. " +
            $"Correct parses: {correctParses}/{totalTests}");
    }

    [Fact]
    public async Task ParseNaturalLanguageAsync_AllTestCasesSucceed()
    {
        // Arrange
        var testCases = AccuracyTestCases().Select(tc => (TestCase)tc[0]).ToList();
        var failures = new List<string>();

        // Act
        foreach (var testCase in testCases)
        {
            try
            {
                var result = await _service.ParseNaturalLanguageAsync(testCase.Input);
                
                if (!result.IsSuccess)
                {
                    failures.Add($"Failed to parse: {testCase.Input} - {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"Exception for: {testCase.Input} - {ex.Message}");
            }
        }

        // Assert
        Assert.Empty(failures);
    }
}
