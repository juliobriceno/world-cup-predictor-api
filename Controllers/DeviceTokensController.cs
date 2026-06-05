using Goal2026API.Api.Data;
using Goal2026API.Api.Data.Entities;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Entities;
using Goal2026API.Common;
using Goal2026API.Entities.FirebasePushNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Api.Controllers;

[ApiController]
[Route("api/device-tokens")]
[Authorize]
public sealed class DeviceTokensController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public DeviceTokensController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private string FirebaseUid => User.GetFirebaseUid();

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDeviceTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Token is required." });
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(x => x.FirebaseUid == FirebaseUid, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var token = request.Token.Trim();

        var existingToken = await _dbContext.UserDeviceTokens
            .SingleOrDefaultAsync(x =>
                x.UserId == user.Id &&
                x.Token == token,
                cancellationToken);

        if (existingToken is null)
        {
            existingToken = new UserDeviceToken
            {
                UserId = user.Id,
                Token = token,
                Channel = "Push",
                Platform = string.IsNullOrWhiteSpace(request.Platform)
                    ? "Web"
                    : request.Platform.Trim(),
                DeviceName = string.IsNullOrWhiteSpace(request.DeviceName)
                    ? null
                    : request.DeviceName.Trim(),
                AppVersion = string.IsNullOrWhiteSpace(request.AppVersion)
                    ? null
                    : request.AppVersion.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow
            };

            _dbContext.UserDeviceTokens.Add(existingToken);
        }
        else
        {
            existingToken.IsActive = true;
            existingToken.Platform = string.IsNullOrWhiteSpace(request.Platform)
                ? "Web"
                : request.Platform.Trim();
            existingToken.DeviceName = string.IsNullOrWhiteSpace(request.DeviceName)
                ? null
                : request.DeviceName.Trim();
            existingToken.AppVersion = string.IsNullOrWhiteSpace(request.AppVersion)
                ? null
                : request.AppVersion.Trim();
            existingToken.LastSeenAt = DateTime.UtcNow;
            existingToken.InvalidatedAt = null;
            existingToken.InvalidReason = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            success = true,
            tokenId = existingToken.Id
        });
    }
}