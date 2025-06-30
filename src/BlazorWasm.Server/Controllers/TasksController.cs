using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorWasm.Shared.Models;
using BlazorWasm.Shared.Enums;
using BlazorWasm.Server.Data;
using BlazorWasm.Shared.DTOs;
using System.Security.Claims;
using TaskStatus = BlazorWasm.Shared.Enums.TaskStatus;
using Markdig;

namespace BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TasksController> _logger;
    private readonly MarkdownPipeline _markdownPipeline;

    public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;

        // Configure Markdown pipeline for security
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .DisableHtml() // Disable raw HTML for security
            .Build();
    }

    /// <summary>
    /// Get all tasks with pagination, search, and filtering
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = "CreatedAt:desc",
        [FromQuery] string? filterStatus = null,
        [FromQuery] string? filterPriority = null,
        [FromQuery] int? assigneeId = null)
    {
        try
        {
            var query = _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .Where(t => !t.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filterStatus) && Enum.TryParse<TaskStatus>(filterStatus, true, out var status))
            {
                query = query.Where(t => t.Status == status);
            }

            // Apply priority filter
            if (!string.IsNullOrEmpty(filterPriority) && Enum.TryParse<Priority>(filterPriority, true, out var priority))
            {
                query = query.Where(t => t.Priority == priority);
            }

            // Apply assignee filter
            if (assigneeId.HasValue)
            {
                query = query.Where(t => t.AssigneeId == assigneeId.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1] : "asc";

                query = sortField.ToLower() switch
                {
                    "title" => sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.Title)
                        : query.OrderBy(t => t.Title),
                    "status" => sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.Status)
                        : query.OrderBy(t => t.Status),
                    "priority" => sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.Priority)
                        : query.OrderBy(t => t.Priority),
                    "duedate" => sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.DueDate)
                        : query.OrderBy(t => t.DueDate),
                    "updatedat" => sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.UpdatedAt)
                        : query.OrderBy(t => t.UpdatedAt),
                    _ => sortDirection.ToLower() == "desc"
                        ? query.OrderByDescending(t => t.CreatedAt)
                        : query.OrderBy(t => t.CreatedAt)
                };
            }

            var totalCount = await query.CountAsync();

            var tasks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee != null ? $"{t.Assignee.FirstName} {t.Assignee.LastName}" : null,
                    CreatorId = t.CreatorId,
                    CreatorName = $"{t.Creator.FirstName} {t.Creator.LastName}",
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            // Add total count to response headers
            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, new { message = "Error retrieving tasks" });
        }
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> GetTask(int id)
    {
        try
        {
            var task = await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .Include(t => t.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
                .Where(t => !t.IsDeleted && t.Id == id)
                .Select(t => new TaskDetailDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee != null ? $"{t.Assignee.FirstName} {t.Assignee.LastName}" : null,
                    CreatorId = t.CreatorId,
                    CreatorName = $"{t.Creator.FirstName} {t.Creator.LastName}",
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Comments = t.Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserId = c.UserId,
                        UserName = $"{c.User.FirstName} {c.User.LastName}",
                        CreatedAt = c.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, new { message = "Error retrieving task" });
        }
    }

    /// <summary>
    /// Get a task for editing (TaskEditDto format)
    /// </summary>
    [HttpGet("{id}/edit")]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> GetTaskForEdit(int id)
    {
        try
        {
            var task = await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .Include(t => t.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
                .Where(t => !t.IsDeleted && t.Id == id)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }

            var taskEditDto = new TaskEditDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                AssigneeId = task.AssigneeId,
                CreatorId = task.CreatorId,
                CreatorName = $"{task.Creator.FirstName} {task.Creator.LastName}",
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Comments = task.Comments.Select(c => new CommentDisplayDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    ContentHtml = Markdown.ToHtml(c.Content, _markdownPipeline),
                    UserId = c.UserId,
                    UserName = $"{c.User.FirstName} {c.User.LastName}",
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return Ok(taskEditDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task for edit {TaskId}", id);
            return StatusCode(500, new { message = "Error retrieving task" });
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Status = createTaskDto.Status,
                Priority = createTaskDto.Priority,
                AssigneeId = createTaskDto.AssigneeId,
                CreatorId = userId,
                DueDate = createTaskDto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Load the full task with navigation properties for response
            var createdTask = await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Creator)
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee != null ? $"{t.Assignee.FirstName} {t.Assignee.LastName}" : null,
                    CreatorId = t.CreatorId,
                    CreatorName = $"{t.Creator.FirstName} {t.Creator.LastName}",
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, createdTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "Error creating task" });
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.IsDeleted)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if user can modify this task (creator or admin)
            if (userRole != "Admin" && task.CreatorId != userId && task.AssigneeId != userId)
            {
                return Forbid();
            }

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.Status = updateTaskDto.Status;
            task.Priority = updateTaskDto.Priority;
            task.AssigneeId = updateTaskDto.AssigneeId;
            task.DueDate = updateTaskDto.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, new { message = "Error updating task" });
        }
    }

    /// <summary>
    /// Soft delete a task
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.IsDeleted)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if user can delete this task (creator or admin)
            if (userRole != "Admin" && task.CreatorId != userId)
            {
                return Forbid();
            }

            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, new { message = "Error deleting task" });
        }
    }

    /// <summary>
    /// Get comments for a specific task
    /// </summary>
    [HttpGet("{id}/comments")]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> GetTaskComments(int id)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.IsDeleted)
            {
                return NotFound("Task not found");
            }

            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TaskId == id && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = comments.Select(c => new CommentDisplayDto
            {
                Id = c.Id,
                Content = c.Content,
                ContentHtml = Markdown.ToHtml(c.Content, _markdownPipeline),
                UserId = c.UserId,
                UserName = $"{c.User.FirstName} {c.User.LastName}",
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(commentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for task {TaskId}", id);
            return StatusCode(500, new { message = "Error retrieving comments" });
        }
    }

    /// <summary>
    /// Add a comment to a task
    /// </summary>
    [HttpPost("{id}/comments")]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> AddTaskComment(int id, [FromBody] CommentCreateDto commentDto)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.IsDeleted)
            {
                return NotFound("Task not found");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var comment = new Comment
            {
                TaskId = id,
                UserId = userId,
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Return the created comment with user information
            var createdComment = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.Id == comment.Id)
                .FirstOrDefaultAsync();

            if (createdComment != null)
            {
                var commentDto2 = new CommentDisplayDto
                {
                    Id = createdComment.Id,
                    Content = createdComment.Content,
                    ContentHtml = Markdown.ToHtml(createdComment.Content, _markdownPipeline),
                    UserId = createdComment.UserId,
                    UserName = $"{createdComment.User.FirstName} {createdComment.User.LastName}",
                    CreatedAt = createdComment.CreatedAt
                };

                _logger.LogInformation("Comment {CommentId} added to task {TaskId} by user {UserId}", comment.Id, id, userId);
                return CreatedAtAction(nameof(GetTaskComments), new { id }, commentDto2);
            }

            return StatusCode(500, new { message = "Error retrieving created comment" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to task {TaskId}", id);
            return StatusCode(500, new { message = "Error adding comment" });
        }
    }
}
