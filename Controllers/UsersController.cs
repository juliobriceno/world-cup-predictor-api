using Goal2026API.Common;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Goal2026API.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserSyncService _userSyncService;

    public UsersController(IUserSyncService userSyncService)
    {
        _userSyncService = userSyncService;
    }

    private string FirebaseUid => User.GetFirebaseUid();

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await _userSyncService.GetByFirebaseUidAsync(
            FirebaseUid,
            cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateMyProfileDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var user = await _userSyncService.UpdateProfileAsync(
            FirebaseUid,
            dto,
            cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }
}