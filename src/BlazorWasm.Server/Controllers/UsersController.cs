using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorWasm.Shared.Models;

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
    /// Get all users - Admin only
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.IsActive,
                    u.CreatedAt
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

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.CreatedAt
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
