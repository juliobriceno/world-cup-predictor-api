using Goal2026API.Api.DTOs.Groups;
using Goal2026API.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Goal2026API.Controllers;

[ApiController]
[Route("api/group-invitations")]
public class GroupInvitationsController : ControllerBase
{
    private readonly IGroupInvitationService _service;

    public GroupInvitationsController(IGroupInvitationService service)
    {
        _service = service;
    }

    private string? FirebaseUid =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("uid");

    [Authorize]
    [HttpGet("my-pending")]
    public async Task<IActionResult> GetMyPending(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FirebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        var result = await _service.GetMyPendingInvitationsAsync(FirebaseUid, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string token, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ResolveAsync(FirebaseUid, token, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex.Message switch
            {
                "TOKEN_REQUIRED" => BadRequest(new { message = "TOKEN_REQUIRED" }),
                "INVITATION_NOT_FOUND" => NotFound(new { message = "INVITATION_NOT_FOUND" }),
                _ => BadRequest(new { message = ex.Message })
            };
        }
    }

    // Used by email-link flow
    [Authorize]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept([FromBody] RespondToGroupInvitationDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FirebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        try
        {
            await _service.AcceptAsync(FirebaseUid, dto.Token, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return ex.Message switch
            {
                "TOKEN_REQUIRED" => BadRequest(new { message = "TOKEN_REQUIRED" }),
                "USER_NOT_FOUND" => NotFound(new { message = "USER_NOT_FOUND" }),
                "INVITATION_NOT_FOUND" => NotFound(new { message = "INVITATION_NOT_FOUND" }),
                "EMAIL_MISMATCH" => Conflict(new { message = "EMAIL_MISMATCH" }),
                "INVITATION_EXPIRED" => Conflict(new { message = "INVITATION_EXPIRED" }),
                "INVITATION_NOT_PENDING" => Conflict(new { message = "INVITATION_NOT_PENDING" }),
                "ALREADY_MEMBER" => Conflict(new { message = "ALREADY_MEMBER" }),
                _ => BadRequest(new { message = ex.Message })
            };
        }
    }

    // Used by email-link flow
    [Authorize]
    [HttpPost("decline")]
    public async Task<IActionResult> Decline([FromBody] RespondToGroupInvitationDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FirebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        try
        {
            await _service.DeclineAsync(FirebaseUid, dto.Token, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return ex.Message switch
            {
                "TOKEN_REQUIRED" => BadRequest(new { message = "TOKEN_REQUIRED" }),
                "USER_NOT_FOUND" => NotFound(new { message = "USER_NOT_FOUND" }),
                "INVITATION_NOT_FOUND" => NotFound(new { message = "INVITATION_NOT_FOUND" }),
                "EMAIL_MISMATCH" => Conflict(new { message = "EMAIL_MISMATCH" }),
                "INVITATION_EXPIRED" => Conflict(new { message = "INVITATION_EXPIRED" }),
                "INVITATION_NOT_PENDING" => Conflict(new { message = "INVITATION_NOT_PENDING" }),
                _ => BadRequest(new { message = ex.Message })
            };
        }
    }

    // Used by dashboard flow
    [Authorize]
    [HttpPost("accept-by-id")]
    public async Task<IActionResult> AcceptById(
        [FromBody] RespondToGroupInvitationByIdDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FirebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        try
        {
            await _service.AcceptByInvitationIdAsync(FirebaseUid, dto.InvitationId, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return ex.Message switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = "USER_NOT_FOUND" }),
                "INVITATION_NOT_FOUND" => NotFound(new { message = "INVITATION_NOT_FOUND" }),
                "EMAIL_MISMATCH" => Conflict(new { message = "EMAIL_MISMATCH" }),
                "INVITATION_EXPIRED" => Conflict(new { message = "INVITATION_EXPIRED" }),
                "INVITATION_NOT_PENDING" => Conflict(new { message = "INVITATION_NOT_PENDING" }),
                "ALREADY_MEMBER" => Conflict(new { message = "ALREADY_MEMBER" }),
                _ => BadRequest(new { message = ex.Message })
            };
        }
    }

    // Used by dashboard flow
    [Authorize]
    [HttpPost("decline-by-id")]
    public async Task<IActionResult> DeclineById(
        [FromBody] RespondToGroupInvitationByIdDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FirebaseUid))
        {
            return Unauthorized(new { message = "Firebase UID claim was not found." });
        }

        try
        {
            await _service.DeclineByInvitationIdAsync(FirebaseUid, dto.InvitationId, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return ex.Message switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = "USER_NOT_FOUND" }),
                "INVITATION_NOT_FOUND" => NotFound(new { message = "INVITATION_NOT_FOUND" }),
                "EMAIL_MISMATCH" => Conflict(new { message = "EMAIL_MISMATCH" }),
                "INVITATION_EXPIRED" => Conflict(new { message = "INVITATION_EXPIRED" }),
                "INVITATION_NOT_PENDING" => Conflict(new { message = "INVITATION_NOT_PENDING" }),
                _ => BadRequest(new { message = ex.Message })
            };
        }
    }
}