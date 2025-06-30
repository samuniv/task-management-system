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
}
