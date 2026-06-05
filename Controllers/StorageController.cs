using Amazon.S3;
using Goal2026API.Api.Data;
using Goal2026API.DTOs.Storage;
using Goal2026API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Goal2026API.Controllers;

[ApiController]
[Route("api/storage")]
[Authorize]
public sealed class StorageController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IStorageService _storageService;

    public StorageController(
        AppDbContext dbContext,
        IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    [HttpPost("group-image-upload-ticket")]
    public async Task<IActionResult> CreateGroupImageUploadTicket(
        [FromBody] RequestGroupImageUploadTicketDto dto,
        CancellationToken cancellationToken)
    {
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("uid");

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        if (dto is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        try
        {
            _storageService.ValidateUploadRequest(dto.ContentType, dto.FileSize);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        string objectKey;

        if (dto.GroupId.HasValue)
        {
            var group = await _dbContext.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    g => g.Id == dto.GroupId.Value && !g.IsDeleted,
                    cancellationToken);

            if (group is null)
            {
                return NotFound(new { message = "Group not found." });
            }

            if (group.OwnerUserId != user.Id)
            {
                return Forbid();
            }

            objectKey = _storageService.BuildFinalGroupImageKey(
                dto.GroupId.Value,
                dto.ContentType);
        }
        else
        {
            objectKey = _storageService.BuildTemporaryGroupImageKey(
                firebaseUid,
                dto.ContentType);
        }

        var ticket = await _storageService.CreateUploadTicketAsync(
            objectKey,
            dto.ContentType,
            dto.FileSize,
            cancellationToken);

        return Ok(ticket);
    }

    [HttpPost("user-image-upload-ticket")]
    public async Task<IActionResult> CreateUserImageUploadTicket(
        [FromBody] RequestUserImageUploadTicketDto dto,
        CancellationToken cancellationToken)
    {
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("uid");

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        if (dto is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        try
        {
            _storageService.ValidateUploadRequest(dto.ContentType, dto.FileSize);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var objectKey = _storageService.BuildUserImageKey(user.Id, dto.ContentType);

        var ticket = await _storageService.CreateUploadTicketAsync(
            objectKey,
            dto.ContentType,
            dto.FileSize,
            cancellationToken);

        return Ok(ticket);
    }
}