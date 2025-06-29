using BlazorWasm.Shared.Models;
using BlazorWasm.Shared.Enums;
using TaskStatus = BlazorWasm.Shared.Enums.TaskStatus;

namespace BlazorWasm.Server.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure the database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Create sample users
        var users = new ApplicationUser[]
        {
            new ApplicationUser
            {
                Email = "admin@taskmanager.com",
                UserName = "admin@taskmanager.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Administrator",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new ApplicationUser
            {
                Email = "john.doe@company.com",
                UserName = "john.doe@company.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                FirstName = "John",
                LastName = "Doe",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                IsActive = true
            },
            new ApplicationUser
            {
                Email = "jane.smith@company.com",
                UserName = "jane.smith@company.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                FirstName = "Jane",
                LastName = "Smith",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                IsActive = true
            },
            new ApplicationUser
            {
                Email = "mike.wilson@company.com",
                UserName = "mike.wilson@company.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                FirstName = "Mike",
                LastName = "Wilson",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Create sample tasks
        var tasks = new TaskItem[]
        {
            new TaskItem
            {
                Title = "Set up development environment",
                Description = "Install and configure all necessary development tools, IDEs, and databases for the project.",
                Status = TaskStatus.Done,
                Priority = Priority.High,
                CreatorId = users[0].Id,
                AssigneeId = users[1].Id,
                DueDate = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-12),
                IsDeleted = false
            },
            new TaskItem
            {
                Title = "Design user authentication system",
                Description = "Create wireframes and technical specifications for user login, registration, and authorization features.",
                Status = TaskStatus.InProgress,
                Priority = Priority.High,
                CreatorId = users[1].Id,
                AssigneeId = users[2].Id,
                DueDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            },
            new TaskItem
            {
                Title = "Implement task CRUD operations",
                Description = "Develop complete Create, Read, Update, Delete functionality for task management with proper validation.",
                Status = TaskStatus.Pending,
                Priority = Priority.Medium,
                CreatorId = users[1].Id,
                AssigneeId = users[3].Id,
                DueDate = DateTime.UtcNow.AddDays(14),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new TaskItem
            {
                Title = "Create dashboard with analytics",
                Description = "Build an interactive dashboard showing task statistics, progress charts, and team performance metrics.",
                Status = TaskStatus.Pending,
                Priority = Priority.Low,
                CreatorId = users[0].Id,
                AssigneeId = users[2].Id,
                DueDate = DateTime.UtcNow.AddDays(21),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            },
            new TaskItem
            {
                Title = "Write unit tests for API endpoints",
                Description = "Develop comprehensive unit tests covering all API endpoints with mock data and edge cases.",
                Status = TaskStatus.Pending,
                Priority = Priority.Medium,
                CreatorId = users[1].Id,
                AssigneeId = users[3].Id,
                DueDate = DateTime.UtcNow.AddDays(28),
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();

        // Create sample comments
        var comments = new Comment[]
        {
            new Comment
            {
                TaskId = tasks[0].Id,
                UserId = users[1].Id,
                Content = "Development environment has been successfully set up. All team members have access.",
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                IsDeleted = false
            },
            new Comment
            {
                TaskId = tasks[0].Id,
                UserId = users[0].Id,
                Content = "Great work! Make sure to document the setup process for future reference.",
                CreatedAt = DateTime.UtcNow.AddDays(-11),
                IsDeleted = false
            },
            new Comment
            {
                TaskId = tasks[1].Id,
                UserId = users[2].Id,
                Content = "Working on the wireframes now. Should have the initial draft ready by tomorrow.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            },
            new Comment
            {
                TaskId = tasks[1].Id,
                UserId = users[1].Id,
                Content = "Looks good so far. Please include password reset functionality in the design.",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new Comment
            {
                TaskId = tasks[2].Id,
                UserId = users[3].Id,
                Content = "Started analyzing the requirements. Will begin implementation next week.",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                IsDeleted = false
            }
        };

        context.Comments.AddRange(comments);
        context.SaveChanges();
    }
}
