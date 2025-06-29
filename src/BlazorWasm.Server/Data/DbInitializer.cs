using BlazorWasm.Shared.Models;
using BlazorWasm.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskStatus = BlazorWasm.Shared.Enums.TaskStatus;

namespace BlazorWasm.Server.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        // Ensure the database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Create roles first
        await CreateRolesAsync(roleManager);

        // Create sample users
        var users = await CreateUsersAsync(userManager);

        // Create sample tasks
        await CreateTasksAsync(context, users);

        // Create sample comments
        await CreateCommentsAsync(context, users);
    }

    private static async Task CreateRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role"
                };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task<List<ApplicationUser>> CreateUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var users = new List<ApplicationUser>();

        var adminUser = new ApplicationUser
        {
            Email = "admin@taskmanager.com",
            UserName = "admin@taskmanager.com",
            FirstName = "System",
            LastName = "Administrator",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            users.Add(adminUser);
        }

        var regularUsers = new[]
        {
            new { Email = "john.doe@company.com", FirstName = "John", LastName = "Doe", Password = "Password123!" },
            new { Email = "jane.smith@company.com", FirstName = "Jane", LastName = "Smith", Password = "Password123!" },
            new { Email = "mike.wilson@company.com", FirstName = "Mike", LastName = "Wilson", Password = "Password123!" }
        };

        foreach (var userData in regularUsers)
        {
            var user = new ApplicationUser
            {
                Email = userData.Email,
                UserName = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                IsActive = true
            };

            var userResult = await userManager.CreateAsync(user, userData.Password);
            if (userResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
                users.Add(user);
            }
        }

        return users;
    }

    private static async Task CreateTasksAsync(ApplicationDbContext context, List<ApplicationUser> users)
    {
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
        await context.SaveChangesAsync();
    }

    private static async Task CreateCommentsAsync(ApplicationDbContext context, List<ApplicationUser> users)
    {
        var tasks = await context.Tasks.ToListAsync();
        
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
        await context.SaveChangesAsync();
    }
}
