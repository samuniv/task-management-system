using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BlazorWasm.Shared.Models;
using BlazorWasm.Shared.DTOs;

namespace BlazorWasm.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Authentication endpoints
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/auth/refresh", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/auth/logout", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Task endpoints
    public async Task<(TaskDto[]? Tasks, int TotalCount)> GetTasksAsync(int page = 1, int pageSize = 10, string? search = null, string? sort = null, string? filterStatus = null)
    {
        try
        {
            var query = $"api/tasks?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(search))
                query += $"&search={Uri.EscapeDataString(search)}";

            if (!string.IsNullOrEmpty(sort))
                query += $"&sort={Uri.EscapeDataString(sort)}";

            if (!string.IsNullOrEmpty(filterStatus))
                query += $"&filterStatus={Uri.EscapeDataString(filterStatus)}";

            var response = await _httpClient.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var tasks = await response.Content.ReadFromJsonAsync<TaskDto[]>(_jsonOptions);

                // Try to get total count from header
                int totalCount = 0;
                if (response.Headers.TryGetValues("X-Total-Count", out var totalCountValues))
                {
                    var totalCountHeader = totalCountValues.FirstOrDefault();
                    if (!string.IsNullOrEmpty(totalCountHeader) && int.TryParse(totalCountHeader, out var count))
                    {
                        totalCount = count;
                    }
                }

                return (tasks, totalCount);
            }

            return (null, 0);
        }
        catch (Exception)
        {
            return (null, 0);
        }
    }

    public async Task<TaskEditDto?> GetTaskForEditAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/tasks/{id}/edit");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TaskEditDto>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<TaskDetailDto?> GetTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/tasks/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TaskDetailDto>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/tasks", createTaskDto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TaskDto>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> UpdateTaskAsync(int id, TaskEditDto taskEditDto)
    {
        try
        {
            var updateDto = new UpdateTaskDto
            {
                Title = taskEditDto.Title,
                Description = taskEditDto.Description,
                Status = taskEditDto.Status,
                Priority = taskEditDto.Priority,
                AssigneeId = taskEditDto.AssigneeId,
                DueDate = taskEditDto.DueDate
            };

            var response = await _httpClient.PutAsJsonAsync($"api/tasks/{id}", updateDto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<NaturalLanguageTaskResponse?> ParseNaturalLanguageTaskAsync(NaturalLanguageTaskRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/tasks/parse-natural-language", request, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NaturalLanguageTaskResponse>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Comment endpoints
    public async Task<CommentDisplayDto[]?> GetTaskCommentsAsync(int taskId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/tasks/{taskId}/comments");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDisplayDto[]>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<CommentDisplayDto?> CreateCommentAsync(CommentCreateDto commentDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/tasks/{commentDto.TaskId}/comments", commentDto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDisplayDto>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // User endpoints
    public async Task<UserDto[]?> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/users");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto[]>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserProfileDto?> GetUserProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/users/profile");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserProfileDto>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Statistics endpoints
    public async Task<TaskStatisticsDto?> GetTaskStatisticsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/tasks/stats");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TaskStatisticsDto>(_jsonOptions);
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    // Export methods for CSV functionality
    public async Task<string> ExportTasksAsync(TaskFilterDto filter)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                queryParams.Add($"search={Uri.EscapeDataString(filter.SearchTerm)}");
            if (filter.Status.HasValue)
                queryParams.Add($"filterStatus={Uri.EscapeDataString(filter.Status.ToString()!)}");
            if (filter.Priority.HasValue)
                queryParams.Add($"filterPriority={Uri.EscapeDataString(filter.Priority.ToString()!)}");
            if (filter.AssigneeId.HasValue)
                queryParams.Add($"assigneeId={filter.AssigneeId}");
            if (filter.CreatorId.HasValue)
                queryParams.Add($"creatorId={filter.CreatorId}");
            if (filter.DueDateFrom.HasValue)
                queryParams.Add($"dueDateFrom={filter.DueDateFrom.Value:yyyy-MM-dd}");
            if (filter.DueDateTo.HasValue)
                queryParams.Add($"dueDateTo={filter.DueDateTo.Value:yyyy-MM-dd}");
            if (filter.CreatedFrom.HasValue)
                queryParams.Add($"createdFrom={filter.CreatedFrom.Value:yyyy-MM-dd}");
            if (filter.CreatedTo.HasValue)
                queryParams.Add($"createdTo={filter.CreatedTo.Value:yyyy-MM-dd}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var url = $"api/tasks/export{queryString}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export tasks: {ex.Message}", ex);
        }
    }

    // Export endpoints
    public async Task<bool> ExportTasksToCsvAsync(
        string? search = null,
        string? sort = null,
        string? filterStatus = null,
        string? filterPriority = null,
        int? assigneeId = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(sort))
                queryParams.Add($"sort={Uri.EscapeDataString(sort)}");
            if (!string.IsNullOrWhiteSpace(filterStatus))
                queryParams.Add($"filterStatus={Uri.EscapeDataString(filterStatus)}");
            if (!string.IsNullOrWhiteSpace(filterPriority))
                queryParams.Add($"filterPriority={Uri.EscapeDataString(filterPriority)}");
            if (assigneeId.HasValue)
                queryParams.Add($"assigneeId={assigneeId}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var url = $"api/tasks/export{queryString}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var csvContent = await response.Content.ReadAsStringAsync();
                var fileName = GetFileNameFromResponse(response) ?? $"tasks-export-{DateTime.Now:yyyy-MM-dd}.csv";
                
                // Use JS interop to trigger download
                await DownloadFileAsync(csvContent, fileName, "text/csv");
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string? GetFileNameFromResponse(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentDisposition?.FileName != null)
        {
            return response.Content.Headers.ContentDisposition.FileName.Trim('"');
        }
        return null;
    }

    private async Task DownloadFileAsync(string content, string fileName, string contentType)
    {
        // This will be implemented using JS interop
        // For now, we'll just return - the actual download will be handled differently
        await Task.CompletedTask;
    }
}
