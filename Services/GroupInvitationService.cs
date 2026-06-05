using Goal2026API.Api.Common;
using Goal2026API.Api.Data;
using Goal2026API.Api.DTOs.Groups;
using Goal2026API.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Goal2026API.Api.Services;

namespace Goal2026API.Api.Services;

public sealed class GroupInvitationService : IGroupInvitationService
{
    private readonly AppDbContext _db;
    private readonly IInvitationTokenService _tokenService;
    private readonly IGroupInvitationEmailService _emailService;
    private readonly FrontendOptions _frontendOptions;

    private readonly ApiNotificationService _notificationService;

    public GroupInvitationService(
        AppDbContext db,
        IInvitationTokenService tokenService,
        IGroupInvitationEmailService emailService,
        IOptions<FrontendOptions> frontendOptions,
        ApiNotificationService notificationService)
    {
        _db = db;
        _tokenService = tokenService;
        _emailService = emailService;
        _frontendOptions = frontendOptions.Value;
        _notificationService = notificationService;
    }

    public async Task<GroupInvitationDto> CreateAsync(
        string firebaseUid,
        int groupId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new Exception("EMAIL_REQUIRED");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            throw new Exception("USER_NOT_FOUND");
        }

        var group = await _db.Groups
            .FirstOrDefaultAsync(x => x.Id == groupId && !x.IsDeleted, cancellationToken);

        if (group is null)
        {
            throw new Exception("GROUP_NOT_FOUND");
        }

        var isOwnerOrMemberWithInvitePermission = await _db.GroupMembers.AnyAsync(x =>
            x.GroupId == groupId &&
            x.UserId == user.Id &&
            !x.IsDeleted,
            cancellationToken);

        var isOwner = group.OwnerUserId == user.Id;

        if (!isOwner && !isOwnerOrMemberWithInvitePermission)
        {
            throw new Exception("FORBIDDEN");
        }

        var invitedUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Email != null && x.Email.ToUpper() == normalizedEmail,
                cancellationToken);

        var isAlreadyMember = invitedUser != null &&
            await _db.GroupMembers.AnyAsync(x =>
                x.GroupId == groupId &&
                x.UserId == invitedUser.Id &&
                !x.IsDeleted,
                cancellationToken);

        if (isAlreadyMember)
        {
            throw new Exception("ALREADY_MEMBER");
        }

        var existingPending = await _db.GroupInvitations.AnyAsync(x =>
            x.GroupId == groupId &&
            x.InvitedEmailNormalized == normalizedEmail &&
            x.Status == GroupInvitationStatuses.Pending &&
            x.ExpiresAtUtc > DateTime.UtcNow &&
            !x.IsDeleted,
            cancellationToken);

        if (existingPending)
        {
            throw new Exception("ALREADY_INVITED");
        }

        var token = _tokenService.GenerateToken();
        var hash = _tokenService.ComputeHash(token);
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddDays(7);

        var entity = new Entities.GroupInvitation
        {
            GroupId = groupId,
            InvitedEmail = email.Trim(),
            InvitedEmailNormalized = normalizedEmail,
            TokenHash = hash,
            ExpiresAtUtc = expiresAtUtc,
            Status = GroupInvitationStatuses.Pending,
            CreatedAtUtc = now,
            CreatedByUserId = user.Id,
            UpdatedAtUtc = now,
            UpdatedByUserId = user.Id
        };

        _db.GroupInvitations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var baseUrl = (_frontendOptions.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new Exception("FRONTEND_BASE_URL_NOT_CONFIGURED");
        }

        var acceptUrl =
            $"{baseUrl}/group-invitations/resolve?token={Uri.EscapeDataString(token)}";

        var invitedByName =
            !string.IsNullOrWhiteSpace(user.Nickname)
                ? user.Nickname
                : user.Email ?? "A Goal2026 user";

        await _emailService.SendInvitationAsync(
            entity.InvitedEmail,
            group.Name,
            invitedByName,
            acceptUrl,
            expiresAtUtc,
            cancellationToken);

        return new GroupInvitationDto
        {
            Id = entity.Id,
            GroupId = entity.GroupId,
            InvitedEmail = entity.InvitedEmail,
            Status = entity.Status,
            ExpiresAtUtc = entity.ExpiresAtUtc
        };
    }

    public async Task<List<MyPendingGroupInvitationDto>> GetMyPendingInvitationsAsync(
        string firebaseUid,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return new List<MyPendingGroupInvitationDto>();
        }

        var now = DateTime.UtcNow;
        var userEmail = user.Email.Trim().ToUpperInvariant();

        var invitations = await _db.GroupInvitations
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status == GroupInvitationStatuses.Pending &&
                x.InvitedEmailNormalized == userEmail)
            .Select(x => new MyPendingGroupInvitationDto
            {
                InvitationId = x.Id,
                GroupId = x.GroupId,
                GroupName = x.Group.Name,
                InvitedEmail = x.InvitedEmail,
                Status = x.Status,
                ExpiresAtUtc = x.ExpiresAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
                IsExpired = x.ExpiresAtUtc <= now
            })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return invitations;
    }

    public async Task<ResolveGroupInvitationResponseDto> ResolveAsync(
        string? firebaseUid,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("TOKEN_REQUIRED");
        }

        var hash = _tokenService.ComputeHash(token);

        var invitation = await _db.GroupInvitations
            .AsNoTracking()
            .Include(x => x.Group)
            .FirstOrDefaultAsync(x => x.TokenHash == hash && !x.IsDeleted, cancellationToken);

        if (invitation is null)
        {
            throw new Exception("INVITATION_NOT_FOUND");
        }

        var now = DateTime.UtcNow;
        var isExpired = invitation.ExpiresAtUtc <= now;

        var result = new ResolveGroupInvitationResponseDto
        {
            GroupId = invitation.GroupId,
            GroupName = invitation.Group.Name,
            InvitedEmail = invitation.InvitedEmail,
            Status = invitation.Status,
            ExpiresAtUtc = invitation.ExpiresAtUtc,
            IsExpired = isExpired,
            RequiresAuthentication = true,
            EmailMatchesAuthenticatedUser = false,
            IsAlreadyMember = false
        };

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return result;
        }

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return result;
        }

        result.RequiresAuthentication = false;
        result.EmailMatchesAuthenticatedUser =
            string.Equals(user.Email, invitation.InvitedEmail, StringComparison.OrdinalIgnoreCase);

        result.IsAlreadyMember = await _db.GroupMembers.AnyAsync(x =>
            x.GroupId == invitation.GroupId &&
            x.UserId == user.Id &&
            !x.IsDeleted,
            cancellationToken);

        return result;
    }

    public async Task AcceptAsync(
        string firebaseUid,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("TOKEN_REQUIRED");
        }

        var hash = _tokenService.ComputeHash(token);

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            throw new Exception("USER_NOT_FOUND");
        }

        var inv = await _db.GroupInvitations
            .FirstOrDefaultAsync(x => x.TokenHash == hash && !x.IsDeleted, cancellationToken);

        if (inv is null)
        {
            throw new Exception("INVITATION_NOT_FOUND");
        }

        await AcceptInternalAsync(user, inv, cancellationToken);
    }

    public async Task DeclineAsync(
        string firebaseUid,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception("TOKEN_REQUIRED");
        }

        var hash = _tokenService.ComputeHash(token);

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            throw new Exception("USER_NOT_FOUND");
        }

        var inv = await _db.GroupInvitations
            .FirstOrDefaultAsync(x => x.TokenHash == hash && !x.IsDeleted, cancellationToken);

        if (inv is null)
        {
            throw new Exception("INVITATION_NOT_FOUND");
        }

        await DeclineInternalAsync(user, inv, cancellationToken);
    }

    public async Task AcceptByInvitationIdAsync(
        string firebaseUid,
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            throw new Exception("USER_NOT_FOUND");
        }

        var inv = await _db.GroupInvitations
            .FirstOrDefaultAsync(x => x.Id == invitationId && !x.IsDeleted, cancellationToken);

        if (inv is null)
        {
            throw new Exception("INVITATION_NOT_FOUND");
        }

        await AcceptInternalAsync(user, inv, cancellationToken);
    }

    public async Task DeclineByInvitationIdAsync(
        string firebaseUid,
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            throw new Exception("USER_NOT_FOUND");
        }

        var inv = await _db.GroupInvitations
            .FirstOrDefaultAsync(x => x.Id == invitationId && !x.IsDeleted, cancellationToken);

        if (inv is null)
        {
            throw new Exception("INVITATION_NOT_FOUND");
        }

        await DeclineInternalAsync(user, inv, cancellationToken);
    }

    private async Task AcceptInternalAsync(
        Entities.User user,
        Entities.GroupInvitation inv,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(user.Email, inv.InvitedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("EMAIL_MISMATCH");
        }

        if (inv.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new Exception("INVITATION_EXPIRED");
        }

        if (inv.Status != GroupInvitationStatuses.Pending)
        {
            throw new Exception("INVITATION_NOT_PENDING");
        }

        var memberExists = await _db.GroupMembers.AnyAsync(x =>
            x.GroupId == inv.GroupId &&
            x.UserId == user.Id &&
            !x.IsDeleted,
            cancellationToken);

        if (memberExists)
        {
            throw new Exception("ALREADY_MEMBER");
        }

        _db.GroupMembers.Add(new Entities.GroupMember
        {
            GroupId = inv.GroupId,
            UserId = user.Id,
            JoinedAtUtc = DateTime.UtcNow,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = user.Id,
            UpdatedAtUtc = DateTime.UtcNow,
            UpdatedByUserId = user.Id
        });

        inv.Status = GroupInvitationStatuses.Accepted;
        inv.AcceptedByUserId = user.Id;
        inv.RespondedAtUtc = DateTime.UtcNow;
        inv.UpdatedAtUtc = DateTime.UtcNow;
        inv.UpdatedByUserId = user.Id;

        await _db.SaveChangesAsync(cancellationToken);

        var group = await _db.Groups
            .AsNoTracking()
            .FirstAsync(x => x.Id == inv.GroupId, cancellationToken);

        await _notificationService.CreateAsync(
            group.OwnerUserId,
            "group-invitation-accepted",
            "New player joined your group",
            $"{user.Nickname ?? user.Email} joined your group {group.Name}.",
            new List<string> { "Email" },
            $"group-invitation-accepted:{inv.Id}",
            cancellationToken);

    }

    private async Task DeclineInternalAsync(
        Entities.User user,
        Entities.GroupInvitation inv,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(user.Email, inv.InvitedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("EMAIL_MISMATCH");
        }

        if (inv.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new Exception("INVITATION_EXPIRED");
        }

        if (inv.Status != GroupInvitationStatuses.Pending)
        {
            throw new Exception("INVITATION_NOT_PENDING");
        }

        inv.Status = GroupInvitationStatuses.Declined;
        inv.RespondedAtUtc = DateTime.UtcNow;
        inv.UpdatedAtUtc = DateTime.UtcNow;
        inv.UpdatedByUserId = user.Id;
        inv.DeclinedByUserId = user.Id;

        await _db.SaveChangesAsync(cancellationToken);

        var group = await _db.Groups
            .AsNoTracking()
            .FirstAsync(x => x.Id == inv.GroupId, cancellationToken);

        await _notificationService.CreateAsync(
            group.OwnerUserId,
            "group-invitation-declined",
            "Invitation declined",
            $"{user.Nickname ?? user.Email} declined the invitation to join {group.Name}.",
            new List<string> { "Email" },
            $"group-invitation-declined:{inv.Id}",
            cancellationToken);

    }
}