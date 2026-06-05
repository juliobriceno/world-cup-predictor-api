using Goal2026API.Api.Data;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Entities;
using Goal2026API.DTOs.Common;
using Goal2026API.DTOs.Groups;
using Goal2026API.Services;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Api.Services;

public sealed class GroupService : IGroupService
{
    private const string WorldCupStartedSettingKey = "WorldCupStarted";

    private readonly AppDbContext _dbContext;
    private readonly IStorageService _storageService;

    public GroupService(
        AppDbContext dbContext,
        IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }


    public async Task<ServiceResult<GroupDto>> CreateGroupAsync(
        string firebaseUid,
        CreateGroupDto dto,
        CancellationToken cancellationToken)
    {
        var user = await GetUserByFirebaseUidAsync(firebaseUid, cancellationToken);
        if (user is null)
        {
            return ServiceResult<GroupDto>.Fail("USER_NOT_FOUND", "User not found.");
        }

        if (dto is null)
        {
            return ServiceResult<GroupDto>.Fail("INVALID_REQUEST", "Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return ServiceResult<GroupDto>.Fail("INVALID_NAME", "Group name is required.");
        }

        if (dto.Scoring is null)
        {
            return ServiceResult<GroupDto>.Fail("INVALID_SCORING", "Scoring configuration is required.");
        }

        var validation = ValidateScoring(dto.Scoring);
        if (!validation.Success)
        {
            return ServiceResult<GroupDto>.Fail(validation.Code, validation.Message);
        }

        var utcNow = DateTime.UtcNow;
        var cleanName = dto.Name.Trim();

        var group = new Group
        {
            Name = cleanName,
            OwnerUserId = user.Id,

            ImageKey = NormalizeOptionalString(dto.ImageKey),
            ImageContentType = NormalizeOptionalString(dto.ImageContentType),
            ImageUpdatedAtUtc = string.IsNullOrWhiteSpace(dto.ImageKey) ? null : utcNow,

            IsDeleted = false,
            CreatedAtUtc = utcNow,
            CreatedByUserId = user.Id,
            UpdatedAtUtc = utcNow,
            UpdatedByUserId = user.Id,
            TimeZoneId = NormalizeOptionalString(dto.TimeZoneId) ?? "America/New_York",

        };

        var member = new GroupMember
        {
            Group = group,
            UserId = user.Id,
            JoinedAtUtc = utcNow,
            IsDeleted = false,
            CreatedAtUtc = utcNow,
            CreatedByUserId = user.Id,
            UpdatedAtUtc = utcNow,
            UpdatedByUserId = user.Id
        };

        var scoringRule = new GroupScoringRule
        {
            Group = group,

            EnableOutcomeRule = dto.Scoring.EnableOutcomeRule,
            OutcomePoints = dto.Scoring.EnableOutcomeRule
                ? dto.Scoring.OutcomePoints
                : null,

            EnableExactScoreRule = dto.Scoring.EnableExactScoreRule,
            ExactHomeGoalsPoints = dto.Scoring.EnableExactScoreRule
                ? dto.Scoring.ExactHomeGoalsPoints
                : null,
            ExactAwayGoalsPoints = dto.Scoring.EnableExactScoreRule
                ? dto.Scoring.ExactAwayGoalsPoints
                : null,
            RequireBothExactScores = dto.Scoring.EnableExactScoreRule
                && dto.Scoring.RequireBothExactScores,

            EnableGoalDifferenceRule = dto.Scoring.EnableGoalDifferenceRule,
            ClosedMatchPoints = dto.Scoring.EnableGoalDifferenceRule
                ? dto.Scoring.ClosedMatchPoints
                : null,
            ComfortableWinPoints = dto.Scoring.EnableGoalDifferenceRule
                ? dto.Scoring.ComfortableWinPoints
                : null,
            BlowoutPoints = dto.Scoring.EnableGoalDifferenceRule
                ? dto.Scoring.BlowoutPoints
                : null,

            CreatedAtUtc = utcNow,
            CreatedByUserId = user.Id,
            UpdatedAtUtc = utcNow,
            UpdatedByUserId = user.Id
        };

        _dbContext.Groups.Add(group);
        _dbContext.GroupMembers.Add(member);
        _dbContext.GroupScoringRules.Add(scoringRule);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await MapToGroupDtoAsync(
            group,
            scoringRule,
            user.Id,
            canModifyRules: true,
            cancellationToken);

        return ServiceResult<GroupDto>.Ok(result, "Group created successfully.");
    }

    public async Task<GroupDto?> GetGroupByIdForUserAsync(
        string firebaseUid,
        int groupId,
        CancellationToken cancellationToken)
    {
        var user = await GetUserByFirebaseUidAsync(firebaseUid, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var group = await _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.ScoringRule)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(
                g => g.Id == groupId &&
                     !g.IsDeleted &&
                     g.Members.Any(m => m.UserId == user.Id && !m.IsDeleted),
                cancellationToken);

        if (group is null || group.ScoringRule is null)
        {
            return null;
        }

        var canModifyRules = !await HasWorldCupStartedAsync(cancellationToken);

        return await MapToGroupDtoAsync(
            group,
            group.ScoringRule,
            user.Id,
            canModifyRules,
            cancellationToken);
    }

    public async Task<ServiceResult<GroupDto>> UpdateGroupAsync(
        string firebaseUid,
        int groupId,
        UpdateGroupDto dto,
        CancellationToken cancellationToken)
    {
        var user = await GetUserByFirebaseUidAsync(firebaseUid, cancellationToken);
        if (user is null)
        {
            return ServiceResult<GroupDto>.Fail("USER_NOT_FOUND", "User not found.");
        }

        if (dto is null)
        {
            return ServiceResult<GroupDto>.Fail("INVALID_REQUEST", "Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return ServiceResult<GroupDto>.Fail("INVALID_NAME", "Group name is required.");
        }

        if (dto.Scoring is null)
        {
            return ServiceResult<GroupDto>.Fail("INVALID_SCORING", "Scoring configuration is required.");
        }

        var group = await _dbContext.Groups
            .Include(g => g.ScoringRule)
            .FirstOrDefaultAsync(
                g => g.Id == groupId && !g.IsDeleted,
                cancellationToken);

        if (group is null || group.ScoringRule is null)
        {
            return ServiceResult<GroupDto>.Fail("NOT_FOUND", "Group not found.");
        }

        if (group.OwnerUserId != user.Id)
        {
            return ServiceResult<GroupDto>.Fail("FORBIDDEN", "Only the group owner can modify the group.");
        }

        if (await HasWorldCupStartedAsync(cancellationToken))
        {
            return ServiceResult<GroupDto>.Fail(
                "WORLD_CUP_STARTED",
                "Group rules can no longer be modified.");
        }

        var validation = ValidateScoring(dto.Scoring);
        if (!validation.Success)
        {
            return ServiceResult<GroupDto>.Fail(validation.Code, validation.Message);
        }

        var utcNow = DateTime.UtcNow;

        group.Name = dto.Name.Trim();
        group.TimeZoneId = NormalizeOptionalString(dto.TimeZoneId) ?? "America/New_York";

        group.UpdatedAtUtc = utcNow;
        group.UpdatedByUserId = user.Id;

        var normalizedImageKey = NormalizeOptionalString(dto.ImageKey);
        var normalizedImageContentType = NormalizeOptionalString(dto.ImageContentType);

        if (!string.IsNullOrWhiteSpace(normalizedImageKey))
        {
            group.ImageKey = normalizedImageKey;
            group.ImageContentType = normalizedImageContentType;
            group.ImageUpdatedAtUtc = utcNow;
        }

        group.ScoringRule.EnableOutcomeRule = dto.Scoring.EnableOutcomeRule;
        group.ScoringRule.OutcomePoints = dto.Scoring.EnableOutcomeRule
            ? dto.Scoring.OutcomePoints
            : null;

        group.ScoringRule.EnableExactScoreRule = dto.Scoring.EnableExactScoreRule;
        group.ScoringRule.ExactHomeGoalsPoints = dto.Scoring.EnableExactScoreRule
            ? dto.Scoring.ExactHomeGoalsPoints
            : null;
        group.ScoringRule.ExactAwayGoalsPoints = dto.Scoring.EnableExactScoreRule
            ? dto.Scoring.ExactAwayGoalsPoints
            : null;
        group.ScoringRule.RequireBothExactScores = dto.Scoring.EnableExactScoreRule
            && dto.Scoring.RequireBothExactScores;

        group.ScoringRule.EnableGoalDifferenceRule = dto.Scoring.EnableGoalDifferenceRule;
        group.ScoringRule.ClosedMatchPoints = dto.Scoring.EnableGoalDifferenceRule
            ? dto.Scoring.ClosedMatchPoints
            : null;
        group.ScoringRule.ComfortableWinPoints = dto.Scoring.EnableGoalDifferenceRule
            ? dto.Scoring.ComfortableWinPoints
            : null;
        group.ScoringRule.BlowoutPoints = dto.Scoring.EnableGoalDifferenceRule
            ? dto.Scoring.BlowoutPoints
            : null;

        group.ScoringRule.UpdatedAtUtc = utcNow;
        group.ScoringRule.UpdatedByUserId = user.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await MapToGroupDtoAsync(
            group,
            group.ScoringRule,
            user.Id,
            canModifyRules: true,
            cancellationToken);

        return ServiceResult<GroupDto>.Ok(result, "Group updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteGroupAsync(
        string firebaseUid,
        int groupId,
        CancellationToken cancellationToken)
    {
        var user = await GetUserByFirebaseUidAsync(firebaseUid, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Fail("USER_NOT_FOUND", "User not found.");
        }

        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(
                g => g.Id == groupId && !g.IsDeleted,
                cancellationToken);

        if (group is null)
        {
            return ServiceResult<bool>.Fail("NOT_FOUND", "Group not found.");
        }

        if (group.OwnerUserId != user.Id)
        {
            return ServiceResult<bool>.Fail("FORBIDDEN", "Only the group owner can delete the group.");
        }

        var utcNow = DateTime.UtcNow;

        group.IsDeleted = true;
        group.UpdatedAtUtc = utcNow;
        group.UpdatedByUserId = user.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Ok(true, "Group deleted successfully.");
    }

    private async Task<User?> GetUserByFirebaseUidAsync(
        string firebaseUid,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.FirebaseUid == firebaseUid,
                cancellationToken);
    }

    private async Task<bool> HasWorldCupStartedAsync(CancellationToken cancellationToken)
    {
        var value = await _dbContext.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == WorldCupStartedSettingKey)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return bool.TryParse(value, out var parsed) && parsed;
    }

    private static ServiceResult<bool> ValidateScoring(GroupScoringRuleDto dto)
    {
        var activeRules = 0;

        if (dto.EnableOutcomeRule)
        {
            activeRules++;

            if (!dto.OutcomePoints.HasValue || dto.OutcomePoints.Value < 1)
            {
                return ServiceResult<bool>.Fail(
                    "INVALID_SCORING",
                    "OutcomePoints must be at least 1 when outcome rule is enabled.");
            }
        }

        if (dto.EnableExactScoreRule)
        {
            activeRules++;

            if (!dto.ExactHomeGoalsPoints.HasValue || dto.ExactHomeGoalsPoints.Value < 1)
            {
                return ServiceResult<bool>.Fail(
                    "INVALID_SCORING",
                    "ExactHomeGoalsPoints must be at least 1 when exact score rule is enabled.");
            }

            if (!dto.ExactAwayGoalsPoints.HasValue || dto.ExactAwayGoalsPoints.Value < 1)
            {
                return ServiceResult<bool>.Fail(
                    "INVALID_SCORING",
                    "ExactAwayGoalsPoints must be at least 1 when exact score rule is enabled.");
            }
        }

        if (dto.EnableGoalDifferenceRule)
        {
            activeRules++;

            if (!dto.ClosedMatchPoints.HasValue || dto.ClosedMatchPoints.Value < 1)
            {
                return ServiceResult<bool>.Fail(
                    "INVALID_SCORING",
                    "ClosedMatchPoints must be at least 1 when goal difference rule is enabled.");
            }

            if (!dto.ComfortableWinPoints.HasValue || dto.ComfortableWinPoints.Value < 1)
            {
                return ServiceResult<bool>.Fail(
                    "INVALID_SCORING",
                    "ComfortableWinPoints must be at least 1 when goal difference rule is enabled.");
            }

            if (!dto.BlowoutPoints.HasValue || dto.BlowoutPoints.Value < 1)
            {
                return ServiceResult<bool>.Fail(
                    "INVALID_SCORING",
                    "BlowoutPoints must be at least 1 when goal difference rule is enabled.");
            }
        }

        if (activeRules < 1 || activeRules > 3)
        {
            return ServiceResult<bool>.Fail(
                "INVALID_SCORING",
                "At least one scoring rule and at most three scoring rules must be enabled.");
        }

        return ServiceResult<bool>.Ok(true);
    }

    private async Task<GroupDto> MapToGroupDtoAsync(
        Group group,
        GroupScoringRule rule,
        int currentUserId,
        bool canModifyRules,
        CancellationToken cancellationToken)
    {
        string? imageUrl = null;

        if (!string.IsNullOrWhiteSpace(group.ImageKey))
        {
            try
            {
                var readUrl = await _storageService.CreateReadUrlAsync(
                    group.ImageKey,
                    cancellationToken);

                imageUrl = readUrl.Url;
            }
            catch
            {
                imageUrl = null;
            }
        }

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            TimeZoneId = group.TimeZoneId,
            OwnerUserId = group.OwnerUserId,
            CanEdit = group.OwnerUserId == currentUserId,
            CanDelete = group.OwnerUserId == currentUserId,
            CanModifyRules = group.OwnerUserId == currentUserId && canModifyRules,
            CreatedAtUtc = group.CreatedAtUtc,
            UpdatedAtUtc = group.UpdatedAtUtc,

            ImageKey = group.ImageKey,
            ImageContentType = group.ImageContentType,
            ImageUpdatedAtUtc = group.ImageUpdatedAtUtc,
            ImageUrl = imageUrl,

            Scoring = new GroupScoringRuleDto
            {
                EnableOutcomeRule = rule.EnableOutcomeRule,
                OutcomePoints = rule.OutcomePoints,

                EnableExactScoreRule = rule.EnableExactScoreRule,
                ExactHomeGoalsPoints = rule.ExactHomeGoalsPoints,
                ExactAwayGoalsPoints = rule.ExactAwayGoalsPoints,
                RequireBothExactScores = rule.RequireBothExactScores,

                EnableGoalDifferenceRule = rule.EnableGoalDifferenceRule,
                ClosedMatchPoints = rule.ClosedMatchPoints,
                ComfortableWinPoints = rule.ComfortableWinPoints,
                BlowoutPoints = rule.BlowoutPoints
            }
        };
    }

    public async Task<List<MyGroupListItemDto>> GetMyGroupsAsync(
        string firebaseUid,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return new List<MyGroupListItemDto>();
        }

        var worldCupStarted = await HasWorldCupStartedAsync(cancellationToken);

        var groups = await _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.Members)
            .Where(g =>
                !g.IsDeleted &&
                (
                    g.OwnerUserId == user.Id ||
                    g.Members.Any(m => m.UserId == user.Id && !m.IsDeleted)
                ))
            .Select(g => new MyGroupListItemDto
            {
                Id = g.Id,
                Name = g.Name,
                OwnerUserId = g.OwnerUserId,
                TimeZoneId = g.TimeZoneId,
                IsOwner = g.OwnerUserId == user.Id,

                CanEdit = g.OwnerUserId == user.Id,
                CanDelete = g.OwnerUserId == user.Id,
                CanInviteMembers = g.OwnerUserId == user.Id && !worldCupStarted,
                CanRemoveMembers = g.OwnerUserId == user.Id,
                CanModifyRules = g.OwnerUserId == user.Id && !worldCupStarted,

                ActiveMemberCount = g.Members.Count(m => !m.IsDeleted),

                ImageKey = g.ImageKey,
                ImageContentType = g.ImageContentType,
                ImageUpdatedAtUtc = g.ImageUpdatedAtUtc,

                ImageUrl = null, // se llena después

                CreatedAtUtc = g.CreatedAtUtc,
                UpdatedAtUtc = g.UpdatedAtUtc
            })
            .OrderByDescending(g => g.IsOwner)
            .ThenByDescending(g => g.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        // 🔥 Generar URLs firmadas (igual que GetGroupById)
        await Task.WhenAll(groups.Select(async g =>
        {
            if (!string.IsNullOrWhiteSpace(g.ImageKey))
            {
                try
                {
                    var readUrl = await _storageService.CreateReadUrlAsync(
                        g.ImageKey,
                        cancellationToken);

                    g.ImageUrl = readUrl.Url;
                }
                catch
                {
                    g.ImageUrl = null;
                }
            }
        }));

        return groups;
    }

    private static string? NormalizeOptionalString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    public async Task<ServiceResult<List<GroupPlayerDto>>> GetGroupPlayersAsync(
        string firebaseUid,
        int groupId,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetUserByFirebaseUidAsync(firebaseUid, cancellationToken);

        if (currentUser is null)
        {
            return ServiceResult<List<GroupPlayerDto>>.Fail(
                "USER_NOT_FOUND",
                "User not found.");
        }

        var group = await _dbContext.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == groupId && !x.IsDeleted,
                cancellationToken);

        if (group is null)
        {
            return ServiceResult<List<GroupPlayerDto>>.Fail(
                "GROUP_NOT_FOUND",
                "Group not found.");
        }

        var isCurrentUserMember = await _dbContext.GroupMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.GroupId == groupId &&
                     x.UserId == currentUser.Id &&
                     !x.IsDeleted,
                cancellationToken);

        if (!isCurrentUserMember && group.OwnerUserId != currentUser.Id)
        {
            return ServiceResult<List<GroupPlayerDto>>.Fail(
                "FORBIDDEN",
                "You are not allowed to view this group.");
        }

        var isOwner = group.OwnerUserId == currentUser.Id;

        var players = await (
            from gm in _dbContext.GroupMembers.AsNoTracking()
            join u in _dbContext.Users.AsNoTracking()
                on gm.UserId equals u.Id
            where gm.GroupId == groupId && !gm.IsDeleted
            orderby u.Nickname, u.Email
            select new
            {
                u.Id,
                u.Email,
                u.Nickname,
                u.PhotoKey,
                gm.JoinedAtUtc,
                gm.IsEnabled
            })
            .ToListAsync(cancellationToken);

        var result = new List<GroupPlayerDto>();

        foreach (var player in players)
        {
            string? photoUrl = null;

            if (!string.IsNullOrWhiteSpace(player.PhotoKey))
            {
                try
                {
                    var readUrl = await _storageService.CreateReadUrlAsync(
                        player.PhotoKey,
                        cancellationToken);

                    photoUrl = readUrl.Url;
                }
                catch
                {
                    photoUrl = null;
                }
            }

            result.Add(new GroupPlayerDto
            {
                UserId = player.Id,
                Email = player.Email,
                Nickname = player.Nickname,
                PhotoUrl = photoUrl,
                JoinedAtUtc = player.JoinedAtUtc,
                IsEnabled = player.IsEnabled,
                IsOwner = player.Id == group.OwnerUserId,
                CanChangeStatus = isOwner && player.Id != group.OwnerUserId
            });
        }

        return ServiceResult<List<GroupPlayerDto>>.Ok(result);
    }

    public async Task<ServiceResult<GroupPlayerDto>> UpdateGroupPlayerStatusAsync(
        string firebaseUid,
        int groupId,
        int userId,
        bool isEnabled,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetUserByFirebaseUidAsync(firebaseUid, cancellationToken);

        if (currentUser is null)
        {
            return ServiceResult<GroupPlayerDto>.Fail(
                "USER_NOT_FOUND",
                "User not found.");
        }

        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(
                x => x.Id == groupId && !x.IsDeleted,
                cancellationToken);

        if (group is null)
        {
            return ServiceResult<GroupPlayerDto>.Fail(
                "GROUP_NOT_FOUND",
                "Group not found.");
        }

        if (group.OwnerUserId != currentUser.Id)
        {
            return ServiceResult<GroupPlayerDto>.Fail(
                "FORBIDDEN",
                "Only the group owner can update player status.");
        }

        if (userId == group.OwnerUserId)
        {
            return ServiceResult<GroupPlayerDto>.Fail(
                "CANNOT_DISABLE_OWNER",
                "The group owner cannot be disabled.");
        }

        var member = await _dbContext.GroupMembers
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.GroupId == groupId &&
                     x.UserId == userId &&
                     !x.IsDeleted,
                cancellationToken);

        if (member is null)
        {
            return ServiceResult<GroupPlayerDto>.Fail(
                "PLAYER_NOT_FOUND",
                "Player not found in this group.");
        }

        var utcNow = DateTime.UtcNow;

        member.IsEnabled = isEnabled;
        member.UpdatedAtUtc = utcNow;
        member.UpdatedByUserId = currentUser.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        string? photoUrl = null;

        if (!string.IsNullOrWhiteSpace(member.User.PhotoKey))
        {
            try
            {
                var readUrl = await _storageService.CreateReadUrlAsync(
                    member.User.PhotoKey,
                    cancellationToken);

                photoUrl = readUrl.Url;
            }
            catch
            {
                photoUrl = null;
            }
        }

        var dto = new GroupPlayerDto
        {
            UserId = member.UserId,
            Email = member.User.Email,
            Nickname = member.User.Nickname,
            PhotoUrl = photoUrl,
            JoinedAtUtc = member.JoinedAtUtc,
            IsEnabled = member.IsEnabled,
            IsOwner = member.UserId == group.OwnerUserId,
            CanChangeStatus = member.UserId != group.OwnerUserId
        };

        return ServiceResult<GroupPlayerDto>.Ok(dto);
    }

}