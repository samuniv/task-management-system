using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorWasm.Shared.Models;
using BlazorWasm.Shared.DTOs;

namespace BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get all users for task assignments - Task managers can access
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanManageTasks")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "Error retrieving users" });
        }
    }

    /// <summary>
    /// Get current user's profile - Any authenticated user
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Policy = "UserAccess")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for user {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            return StatusCode(500, new { message = "Error retrieving profile" });
        }
    }

    /// <summary>
    /// Deactivate a user - Admin only
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} deactivated by {AdminId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Ok(new { message = "User deactivated successfully" });
            }

            return BadRequest(new { message = "Failed to deactivate user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, new { message = "Error deactivating user" });
        }
    }

    /// <summary>
    /// Activate a user - Admin only
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} activated by {AdminId}", id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return Ok(new { message = "User activated successfully" });
            }

            return BadRequest(new { message = "Failed to activate user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return StatusCode(500, new { message = "Error activating user" });
        }
    }
}
