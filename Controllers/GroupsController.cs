using Goal2026API.Api.Data;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.DTOs.Groups;
using Goal2026API.Api.Services;
using Goal2026API.Common;
using Goal2026API.DTOs.Groups;
using Goal2026API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace Goal2026API.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public sealed class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IGroupStandingsService _groupStandingsService;
    private readonly IGroupInvitationService _groupInvitationService;
    private readonly AppDbContext _dbContext;

    public GroupsController(
        IGroupService groupService,
        IGroupStandingsService groupStandingsService,
        IGroupInvitationService groupInvitationService,
        AppDbContext dbContext)
    {
        _groupService = groupService;
        _groupStandingsService = groupStandingsService;
        _groupInvitationService = groupInvitationService;
        _dbContext = dbContext;
    }

    private string FirebaseUid => User.GetFirebaseUid();

    [HttpGet("my-groups")]
    public async Task<IActionResult> GetMyGroups(CancellationToken cancellationToken)
    {
        var result = await _groupService.GetMyGroupsAsync(FirebaseUid, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateGroupDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var result = await _groupService.CreateGroupAsync(
            FirebaseUid,
            dto,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        return CreatedAtAction(
            nameof(GetGroupById),
            new { groupId = result.Data!.Id },
            result.Data);
    }

    [HttpGet("{groupId:int}")]
    public async Task<IActionResult> GetGroupById(
        int groupId,
        CancellationToken cancellationToken)
    {
        var result = await _groupService.GetGroupByIdForUserAsync(
            FirebaseUid,
            groupId,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "Group not found." });
        }

        return Ok(result);
    }

    [HttpPut("{groupId:int}")]
    public async Task<IActionResult> UpdateGroup(
        int groupId,
        [FromBody] UpdateGroupDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var result = await _groupService.UpdateGroupAsync(
            FirebaseUid,
            groupId,
            dto,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = result.Message }),
                "NOT_FOUND" => NotFound(new { message = result.Message }),
                "FORBIDDEN" => Forbid(),
                "WORLD_CUP_STARTED" => Conflict(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        return Ok(result.Data);
    }

    [HttpDelete("{groupId:int}")]
    public async Task<IActionResult> DeleteGroup(
        int groupId,
        CancellationToken cancellationToken)
    {
        var result = await _groupService.DeleteGroupAsync(
            FirebaseUid,
            groupId,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = result.Message }),
                "NOT_FOUND" => NotFound(new { message = result.Message }),
                "FORBIDDEN" => Forbid(),
                _ => BadRequest(new { message = result.Message })
            };
        }

        return NoContent();
    }

    [EnableRateLimiting("heavy-read")]
    [HttpGet("{groupId:int}/standings")]
    public async Task<IActionResult> GetStandings(
        int groupId,
        [FromQuery] GroupStandingsMode mode,
        CancellationToken cancellationToken)
    {
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(firebaseUid))
        {
            return Unauthorized("User UID not found.");
        }

        var result = await _groupStandingsService.GetGroupStandingsAsync(
            groupId,
            firebaseUid,
            mode,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{groupId:int}/invitations")]
    public async Task<IActionResult> CreateInvitation(
        int groupId,
        [FromBody] CreateGroupInvitationDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        try
        {
            var result = await _groupInvitationService.CreateAsync(
                FirebaseUid,
                groupId,
                dto.Email,
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return ex.Message switch
            {
                "EMAIL_REQUIRED" => BadRequest(new { message = "EMAIL_REQUIRED" }),
                "USER_NOT_FOUND" => NotFound(new { message = "USER_NOT_FOUND" }),
                "GROUP_NOT_FOUND" => NotFound(new { message = "GROUP_NOT_FOUND" }),
                "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, new { message = "FORBIDDEN" }),
                "ALREADY_MEMBER" => Conflict(new { message = "ALREADY_MEMBER" }),
                "ALREADY_INVITED" => Conflict(new { message = "ALREADY_INVITED" }),
                "FRONTEND_BASE_URL_NOT_CONFIGURED" => StatusCode(StatusCodes.Status500InternalServerError, new { message = "FRONTEND_BASE_URL_NOT_CONFIGURED" }),
                _ => BadRequest(new { message = ex.Message })
            };
        }
    }

    [EnableRateLimiting("invite-emails")]
    [HttpPost("{groupId:int}/invitations/bulk")]
    public async Task<IActionResult> CreateInvitationsBulk(
        int groupId,
        [FromBody] CreateGroupInvitationsBulkDto dto,
        CancellationToken cancellationToken)
    {
        const int MaxEmailsPerRequest = 10;
        const int MaxEmailsPerHour = 30;
        const int MaxEmailsPerDay = 50;
        const int DelayBetweenEmailsMs = 500;

        if (dto == null || dto.Emails == null || !dto.Emails.Any())
        {
            return BadRequest(new { message = "Emails are required." });
        }

        var emails = dto.Emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLower())
            .Distinct()
            .ToList();

        if (!emails.Any())
        {
            return BadRequest(new { message = "No valid emails provided." });
        }

        if (emails.Count > MaxEmailsPerRequest)
        {
            return BadRequest(new
            {
                message = "MAX_EMAILS_PER_REQUEST",
                max = MaxEmailsPerRequest
            });
        }

        var currentUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == FirebaseUid, cancellationToken);

        if (currentUser == null)
        {
            return Unauthorized(new { message = "USER_NOT_FOUND" });
        }

        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);
        var oneDayAgo = now.AddDays(-1);

        var sentLastHour = await _dbContext.GroupInvitations.CountAsync(x =>
            x.CreatedByUserId == currentUser.Id &&
            x.CreatedAtUtc >= oneHourAgo &&
            !x.IsDeleted,
            cancellationToken);

        var sentLastDay = await _dbContext.GroupInvitations.CountAsync(x =>
            x.CreatedByUserId == currentUser.Id &&
            x.CreatedAtUtc >= oneDayAgo &&
            !x.IsDeleted,
            cancellationToken);

        if (sentLastHour + emails.Count > MaxEmailsPerHour)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "INVITATION_RATE_LIMIT_HOUR",
                limit = MaxEmailsPerHour,
                used = sentLastHour,
                requested = emails.Count
            });
        }

        if (sentLastDay + emails.Count > MaxEmailsPerDay)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "INVITATION_RATE_LIMIT_DAY",
                limit = MaxEmailsPerDay,
                used = sentLastDay,
                requested = emails.Count
            });
        }

        var result = new BulkInvitationResultDto
        {
            Total = emails.Count
        };

        for (var i = 0; i < emails.Count; i++)
        {
            var email = emails[i];

            try
            {
                await _groupInvitationService.CreateAsync(
                    FirebaseUid,
                    groupId,
                    email,
                    cancellationToken);

                result.SuccessCount++;
                result.SuccessfulEmails.Add(email);
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(new BulkInvitationErrorDto
                {
                    Email = email,
                    Error = ex.Message
                });
            }

            if (i < emails.Count - 1)
            {
                await Task.Delay(DelayBetweenEmailsMs, cancellationToken);
            }
        }

        return Ok(result);
    }

    [HttpGet("{groupId:int}/players")]
    public async Task<IActionResult> GetGroupPlayers(
        int groupId,
        CancellationToken cancellationToken)
    {
        var result = await _groupService.GetGroupPlayersAsync(
            FirebaseUid,
            groupId,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = result.Message }),
                "GROUP_NOT_FOUND" => NotFound(new { message = result.Message }),
                "FORBIDDEN" => Forbid(),
                _ => BadRequest(new { message = result.Message })
            };
        }

        return Ok(result.Data);
    }

    [HttpPatch("{groupId:int}/players/{userId:int}/status")]
    public async Task<IActionResult> UpdateGroupPlayerStatus(
        int groupId,
        int userId,
        [FromBody] UpdateGroupPlayerStatusDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _groupService.UpdateGroupPlayerStatusAsync(
            FirebaseUid,
            groupId,
            userId,
            dto.IsEnabled,
            cancellationToken);

        if (!result.Success)
        {
            return result.Code switch
            {
                "USER_NOT_FOUND" => NotFound(new { message = result.Message }),
                "GROUP_NOT_FOUND" => NotFound(new { message = result.Message }),
                "PLAYER_NOT_FOUND" => NotFound(new { message = result.Message }),
                "FORBIDDEN" => Forbid(),
                "CANNOT_DISABLE_OWNER" => Conflict(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        return Ok(result.Data);
    }

    [HttpGet("{groupId:int}/predictions/export")]
    public async Task<IActionResult> ExportPredictionsCsv(
        int groupId,
        CancellationToken cancellationToken)
    {
        var firebaseUid = User.GetFirebaseUid();

        // 🔒 Validar usuario
        var currentUser = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (currentUser == null)
            return Unauthorized();

        // 🔒 Validar que es OWNER
        var isOwner = await _dbContext.Groups
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == groupId &&
                x.OwnerUserId == currentUser.Id &&
                !x.IsDeleted,
                cancellationToken);

        if (!isOwner)
            return Forbid();

        // 👥 Miembros del grupo
        var members = await (
            from gm in _dbContext.GroupMembers
            join u in _dbContext.Users on gm.UserId equals u.Id
            where gm.GroupId == groupId && !gm.IsDeleted
            select new
            {
                u.Id,
                u.Email,
                Name = u.Nickname ?? u.Email
            }
        ).ToListAsync(cancellationToken);

        var memberIds = members.Select(x => x.Id).ToList();

        // ⚽ Partidos
        var matches = await _dbContext.Matches
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.MatchNumber,
                x.MatchDateUtc,
                x.StageCode,
                x.HomeTeam,
                x.AwayTeam
            })
            .OrderBy(x => x.MatchDateUtc)
            .ThenBy(x => x.MatchNumber)
            .ToListAsync(cancellationToken);

        var matchIds = matches.Select(x => x.Id).ToList();

        // 📊 Pronósticos
        var predictions = await _dbContext.UserMatchPredictions
            .Where(x =>
                memberIds.Contains(x.UserId) &&
                matchIds.Contains(x.MatchId))
            .ToListAsync(cancellationToken);

        var predictionMap = predictions
            .GroupBy(x => new { x.UserId, x.MatchId })
            .ToDictionary(
                g => (g.Key.UserId, g.Key.MatchId),
                g => g.Last());

        // 🧾 Construir CSV
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("PlayerName,Email,MatchDate,HomeTeam,AwayTeam,PredictedHomeGoals,PredictedAwayGoals,HasPrediction");

        foreach (var member in members)
        {
            foreach (var match in matches.OrderBy(x => x.MatchDateUtc))
            {
                predictionMap.TryGetValue((member.Id, match.Id), out var p);

                sb.AppendLine(string.Join(",",
                    Escape(member.Name),
                    Escape(member.Email),
                    match.MatchDateUtc.ToString("yyyy-MM-dd HH:mm"),
                    Escape(match.HomeTeam),
                    Escape(match.AwayTeam),
                    p?.PredictedHomeGoals?.ToString() ?? "",
                    p?.PredictedAwayGoals?.ToString() ?? "",
                    p?.HasPrediction == true ? "YES" : "NO"
                ));
            }
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

        return File(
            bytes,
            "text/csv",
            $"group-{groupId}-predictions.csv");
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    [EnableRateLimiting("heavy-read")]
    [HttpGet("{groupId:int}/dashboard")]
    public async Task<IActionResult> GetDashboard(
        int groupId,
        [FromQuery] GroupStandingsMode mode,
        CancellationToken cancellationToken)
    {
        var firebaseUid = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(firebaseUid))
            return Unauthorized();

        var result = await _groupStandingsService
            .GetGroupDashboardAsync(groupId, firebaseUid, mode, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

}