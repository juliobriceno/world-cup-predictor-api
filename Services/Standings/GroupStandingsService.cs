using Goal2026API.Api.Data;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.DTOs.Groups;
using Goal2026API.Api.Entities;
using Goal2026API.Services;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Api.Services;

public sealed class GroupStandingsService : IGroupStandingsService
{
    private const string FirstRoundStageCode = "GROUP";
    private const string WorldCupStartedKey = "WorldCupStarted";

    private readonly AppDbContext _dbContext;
    private readonly IStorageService _storageService;

    public GroupStandingsService(AppDbContext dbContext, IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<GroupStandingsResponseDto?> GetGroupStandingsAsync(
        int groupId,
        string firebaseUid,
        GroupStandingsMode mode,
        CancellationToken cancellationToken)
    {
        var currentUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (currentUser is null) return null;

        var group = await _dbContext.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == groupId && !x.IsDeleted, cancellationToken);

        if (group is null) return null;

        var isCurrentUserMember = await _dbContext.GroupMembers
            .AsNoTracking()
            .AnyAsync(x => x.GroupId == groupId && x.UserId == currentUser.Id && !x.IsDeleted, cancellationToken);

        if (!isCurrentUserMember) return null;

        var worldCupStarted = await GetWorldCupStartedAsync(cancellationToken);

        var scoringRule = await _dbContext.GroupScoringRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GroupId == groupId, cancellationToken);

        if (scoringRule is null)
        {
            return new GroupStandingsResponseDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Mode = mode,
                WorldCupStarted = worldCupStarted,
                EvaluatedMatches = 0,
                Standings = new()
            };
        }

        var members = await (
            from gm in _dbContext.GroupMembers.AsNoTracking()
            join u in _dbContext.Users.AsNoTracking() on gm.UserId equals u.Id
            where gm.GroupId == groupId && !gm.IsDeleted && gm.IsEnabled
            select new MemberInfo
            {
                UserId = u.Id,
                Email = u.Email,
                Nickname = string.IsNullOrWhiteSpace(u.Nickname)
                ? u.Email
                : u.Nickname,
                PhotoKey = u.PhotoKey
            }).ToListAsync(cancellationToken);

        if (members.Count == 0)
        {
            return new GroupStandingsResponseDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Mode = mode,
                WorldCupStarted = worldCupStarted,
                EvaluatedMatches = 0,
                Standings = new()
            };
        }

        // 🚀 OPTIMIZACIÓN PRO
        if (worldCupStarted && mode == GroupStandingsMode.Official)
        {
            var hasMissingMatches = await _dbContext.Matches
                .AsNoTracking()
                .Where(m =>
                    m.StageCode == FirstRoundStageCode &&
                    m.IsActive &&
                    m.IsFinished &&
                    m.HomeTeamGoals.HasValue &&
                    m.AwayTeamGoals.HasValue)
                .AnyAsync(m =>
                    !_dbContext.GroupUserMatchScores
                        .AsNoTracking()
                        .Any(s => s.GroupId == groupId && s.MatchId == m.Id),
                    cancellationToken);

            if (hasMissingMatches)
            {
                await PersistMissingOfficialScoresAsync(
                    groupId,
                    scoringRule,
                    members,
                    cancellationToken);
            }

            // 🔥 LEER DESDE TABLA PERSISTIDA
            var scoreRows = await _dbContext.GroupUserMatchScores
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .GroupBy(x => x.UserId)
                .Select(x => new
                {
                    UserId = x.Key,
                    Points = x.Sum(s => s.Points)
                })
                .ToListAsync(cancellationToken);

            var scoreMap = scoreRows.ToDictionary(x => x.UserId, x => x.Points);

            var rows = new List<GroupStandingRowDto>();

            foreach (var member in members)
            {
                string? photoUrl = null;

                if (!string.IsNullOrWhiteSpace(member.PhotoKey))
                {
                    try
                    {
                        var readUrl = await _storageService.CreateReadUrlAsync(
                            member.PhotoKey,
                            cancellationToken);

                        photoUrl = readUrl.Url;
                    }
                    catch { }
                }

                rows.Add(new GroupStandingRowDto
                {
                    UserId = member.UserId,
                    Email = member.Email,
                    Nickname = member.Nickname,
                    Points = scoreMap.TryGetValue(member.UserId, out var p) ? p : 0,
                    PhotoUrl = photoUrl
                });
            }

            var ordered = rows
                .OrderByDescending(x => x.Points)
                .ThenBy(x => string.IsNullOrWhiteSpace(x.Nickname) ? "~" : x.Nickname)
                .ThenBy(x => x.Email)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
                ordered[i].Position = i + 1;

            return new GroupStandingsResponseDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Mode = mode,
                WorldCupStarted = worldCupStarted,
                EvaluatedMatches = await _dbContext.GroupUserMatchScores
                    .Where(x => x.GroupId == groupId)
                    .Select(x => x.MatchId)
                    .Distinct()
                    .CountAsync(cancellationToken),
                Standings = ordered
            };
        }

        // 👉 fallback (Simulation o pre-worldcup)
        return await CalculateOnTheFlyAsync(
            group,
            members,
            scoringRule,
            mode,
            worldCupStarted,
            currentUser.Id,
            cancellationToken);
    }



    // 🔥 MÉTODO ORIGINAL (sin tocar lógica)
    private async Task<GroupStandingsResponseDto> CalculateOnTheFlyAsync(
        Group group,
        List<MemberInfo> members,
        GroupScoringRule scoringRule,
        GroupStandingsMode mode,
        bool worldCupStarted,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var memberUserIds = members.Select(x => x.UserId).ToList();

        // 1. Obtener partidos base
        // 1. Get base matches
        var matchesQuery = _dbContext.Matches
            .AsNoTracking()
            .Where(x =>
                x.StageCode == FirstRoundStageCode &&
                x.IsActive);

        if (mode == GroupStandingsMode.Official)
        {
            matchesQuery = matchesQuery.Where(x =>
                x.IsFinished &&
                x.HomeTeamGoals.HasValue &&
                x.AwayTeamGoals.HasValue);
        }
        else if (mode == GroupStandingsMode.Simulation)
        {
            matchesQuery = matchesQuery.Where(x =>
                _dbContext.UserMatchSimulations.Any(s =>
                    s.UserId == currentUserId &&
                    s.MatchId == x.Id &&
                    s.HasSimulation &&
                    s.SimulatedHomeGoals.HasValue &&
                    s.SimulatedAwayGoals.HasValue));
        }

        var matches = await matchesQuery
            .Select(x => new
            {
                x.Id,
                x.HomeTeamGoals,
                x.AwayTeamGoals
            })
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return new GroupStandingsResponseDto
            {
                GroupId = group.Id,
                GroupName = group.Name,
                Mode = mode,
                WorldCupStarted = worldCupStarted,
                EvaluatedMatches = 0,
                Standings = new()
            };
        }

        var matchIds = matches.Select(x => x.Id).ToList();

        // 2. Predicciones de TODOS los miembros
        var predictions = await _dbContext.UserMatchPredictions
            .AsNoTracking()
            .Where(x =>
                memberUserIds.Contains(x.UserId) &&
                matchIds.Contains(x.MatchId))
            .ToListAsync(cancellationToken);

        var predictionMap = predictions
            .GroupBy(x => new { x.UserId, x.MatchId })
            .ToDictionary(x => (x.Key.UserId, x.Key.MatchId), x => x.Last());

        // 3. Simulaciones SOLO del usuario actual (si aplica)
        Dictionary<int, UserMatchSimulation>? simulationMap = null;

        if (mode == GroupStandingsMode.Simulation)
        {
            var simulations = await _dbContext.UserMatchSimulations
                .AsNoTracking()
                .Where(x =>
                    x.UserId == currentUserId &&
                    matchIds.Contains(x.MatchId) &&
                    x.HasSimulation)
                .ToListAsync(cancellationToken);

            simulationMap = simulations.ToDictionary(x => x.MatchId);
        }

        var rows = new List<GroupStandingRowDto>();

        // 4. Calcular puntos por miembro
        foreach (var member in members)
        {
            var totalPoints = 0;

            foreach (var match in matches)
            {
                if (!predictionMap.TryGetValue((member.UserId, match.Id), out var prediction))
                    continue;

                var hasValidPrediction =
                    prediction.HasPrediction &&
                    prediction.PredictedHomeGoals.HasValue &&
                    prediction.PredictedAwayGoals.HasValue;

                if (!hasValidPrediction)
                    continue;

                int actualHome;
                int actualAway;

                if (mode == GroupStandingsMode.Simulation)
                {
                    if (simulationMap == null ||
                        !simulationMap.TryGetValue(match.Id, out var sim) ||
                        !sim.SimulatedHomeGoals.HasValue ||
                        !sim.SimulatedAwayGoals.HasValue)
                    {
                        continue; // no hay simulación → no se evalúa
                    }

                    actualHome = sim.SimulatedHomeGoals.Value;
                    actualAway = sim.SimulatedAwayGoals.Value;
                }
                else
                {
                    actualHome = match.HomeTeamGoals!.Value;
                    actualAway = match.AwayTeamGoals!.Value;
                }

                var breakdown = CalculateMatchScoreBreakdown(
                    scoringRule,
                    prediction.PredictedHomeGoals!.Value,
                    prediction.PredictedAwayGoals!.Value,
                    actualHome,
                    actualAway);

                totalPoints += breakdown.TotalPoints;
            }

            // 5. Foto
            string? photoUrl = null;

            if (!string.IsNullOrWhiteSpace(member.PhotoKey))
            {
                try
                {
                    var readUrl = await _storageService.CreateReadUrlAsync(
                        member.PhotoKey,
                        cancellationToken);

                    photoUrl = readUrl.Url;
                }
                catch { }
            }

            rows.Add(new GroupStandingRowDto
            {
                UserId = member.UserId,
                Email = member.Email,
                Nickname = member.Nickname,
                Points = totalPoints,
                PhotoUrl = photoUrl
            });
        }

        // 6. Ordenar ranking
        var ordered = rows
            .OrderByDescending(x => x.Points)
            .ThenBy(x => string.IsNullOrWhiteSpace(x.Nickname) ? "~" : x.Nickname)
            .ThenBy(x => x.Email)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
            ordered[i].Position = i + 1;

        return new GroupStandingsResponseDto
        {
            GroupId = group.Id,
            GroupName = group.Name,
            Mode = mode,
            WorldCupStarted = worldCupStarted,
            EvaluatedMatches = matches.Count,
            Standings = ordered
        };
    }



    private async Task<bool> GetWorldCupStartedAsync(CancellationToken cancellationToken)
    {
        var value = await _dbContext.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == WorldCupStartedKey)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return bool.TryParse(value, out var parsed) && parsed;
    }

    // 🔥 Persistencia incremental (igual que antes)
    private async Task PersistMissingOfficialScoresAsync(
        int groupId,
        GroupScoringRule scoringRule,
        List<MemberInfo> members,
        CancellationToken cancellationToken)
    {
        var memberUserIds = members.Select(x => x.UserId).ToList();

        var finishedMatches = await _dbContext.Matches
            .AsNoTracking()
            .Where(x =>
                x.StageCode == FirstRoundStageCode &&
                x.IsActive &&
                x.IsFinished &&
                x.HomeTeamGoals.HasValue &&
                x.AwayTeamGoals.HasValue)
            .Select(x => new
            {
                x.Id,
                x.HomeTeamGoals,
                x.AwayTeamGoals
            })
            .ToListAsync(cancellationToken);

        if (finishedMatches.Count == 0 || memberUserIds.Count == 0)
            return;

        var finishedMatchIds = finishedMatches.Select(x => x.Id).ToList();

        var alreadyPersisted = await _dbContext.GroupUserMatchScores
            .AsNoTracking()
            .Where(x =>
                x.GroupId == groupId &&
                finishedMatchIds.Contains(x.MatchId))
            .Select(x => new { x.UserId, x.MatchId })
            .ToListAsync(cancellationToken);

        var alreadySet = alreadyPersisted
            .Select(x => (x.UserId, x.MatchId))
            .ToHashSet();

        var predictions = await _dbContext.UserMatchPredictions
            .AsNoTracking()
            .Where(x =>
                memberUserIds.Contains(x.UserId) &&
                finishedMatchIds.Contains(x.MatchId))
            .ToListAsync(cancellationToken);

        var predictionMap = predictions
            .GroupBy(x => new { x.UserId, x.MatchId })
            .ToDictionary(x => (x.Key.UserId, x.Key.MatchId), x => x.Last());

        var now = DateTime.UtcNow;
        var newRows = new List<GroupUserMatchScore>();

        foreach (var match in finishedMatches)
        {
            var officialHome = match.HomeTeamGoals!.Value;
            var officialAway = match.AwayTeamGoals!.Value;

            foreach (var member in members)
            {
                if (alreadySet.Contains((member.UserId, match.Id)))
                    continue;

                predictionMap.TryGetValue((member.UserId, match.Id), out var prediction);

                var hasValidPrediction =
                    prediction != null &&
                    prediction.HasPrediction &&
                    prediction.PredictedHomeGoals.HasValue &&
                    prediction.PredictedAwayGoals.HasValue;

                var breakdown = hasValidPrediction
                    ? CalculateMatchScoreBreakdown(
                        scoringRule,
                        prediction!.PredictedHomeGoals!.Value,
                        prediction.PredictedAwayGoals!.Value,
                        officialHome,
                        officialAway)
                    : new MatchScoreBreakdown();

                newRows.Add(new GroupUserMatchScore
                {
                    GroupId = groupId,
                    UserId = member.UserId,
                    MatchId = match.Id,

                    Points = breakdown.TotalPoints,
                    OutcomePoints = breakdown.OutcomePoints,
                    ExactHomeGoalsPoints = breakdown.ExactHomeGoalsPoints,
                    ExactAwayGoalsPoints = breakdown.ExactAwayGoalsPoints,
                    CategoryPoints = breakdown.CategoryPoints,

                    PredictedHomeGoals = prediction?.PredictedHomeGoals,
                    PredictedAwayGoals = prediction?.PredictedAwayGoals,

                    OfficialHomeGoals = officialHome,
                    OfficialAwayGoals = officialAway,

                    CalculatedAtUtc = now
                });
            }
        }

        if (newRows.Count == 0)
            return;

        _dbContext.GroupUserMatchScores.AddRange(newRows);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            await RebuildGroupJourneyStandingSnapshotsAsync(
                groupId,
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
        }

    }

    private async Task RebuildGroupJourneyStandingSnapshotsAsync(
        int groupId,
        CancellationToken cancellationToken)
    {
        var groupTimeZoneId = await _dbContext.Groups
            .AsNoTracking()
            .Where(x => x.Id == groupId)
            .Select(x => x.TimeZoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? "America/New_York";

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(groupTimeZoneId);

        var rawData = await (
            from score in _dbContext.GroupUserMatchScores.AsNoTracking()
            join match in _dbContext.Matches.AsNoTracking()
                on score.MatchId equals match.Id
            where score.GroupId == groupId
            select new
            {
                score.UserId,
                score.Points,
                MatchId = match.Id,
                match.MatchDateUtc
            }
        ).ToListAsync(cancellationToken);

        var data = rawData
            .GroupBy(x => new
            {
                x.UserId,
                Date = TimeZoneInfo
                    .ConvertTimeFromUtc(
                        DateTime.SpecifyKind(
                            x.MatchDateUtc,
                            DateTimeKind.Utc),
                        timeZone)
                    .Date
            })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                JourneyDate = g.Key.Date,
                PointsOfDay = g.Sum(x => x.Points),
                LastMatchId = g.Max(x => x.MatchId)
            })
            .ToList();



        var existingRows = await _dbContext.GroupUserJourneyStandingSnapshots
            .Where(x => x.GroupId == groupId)
            .ToListAsync(cancellationToken);

        if (data.Count == 0)
        {
            if (existingRows.Count > 0)
            {
                _dbContext.GroupUserJourneyStandingSnapshots.RemoveRange(existingRows);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var journeyDates = data
            .Select(x => x.JourneyDate)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var journeyMap = journeyDates
            .Select((date, index) => new
            {
                Date = date,
                JourneyNumber = index + 1
            })
            .ToDictionary(x => x.Date, x => x.JourneyNumber);

        var userIds = data
            .Select(x => x.UserId)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var pointsMap = data.ToDictionary(
            x => (x.UserId, x.JourneyDate),
            x => new
            {
                x.PointsOfDay,
                x.LastMatchId
            });

        var calculatedRows = new List<GroupUserJourneyStandingSnapshot>();

        foreach (var userId in userIds)
        {
            var cumulativePoints = 0;

            foreach (var journeyDate in journeyDates)
            {
                pointsMap.TryGetValue(
                    (userId, journeyDate),
                    out var dayData);

                var pointsOfDay = dayData?.PointsOfDay ?? 0;
                cumulativePoints += pointsOfDay;

                calculatedRows.Add(new GroupUserJourneyStandingSnapshot
                {
                    GroupId = groupId,
                    UserId = userId,
                    JourneyNumber = journeyMap[journeyDate],
                    JourneyDate = journeyDate,
                    PointsOfDay = pointsOfDay,
                    CumulativePoints = cumulativePoints,
                    LastMatchId = dayData?.LastMatchId,
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }
        }

        foreach (var journeyNumber in calculatedRows
            .Select(x => x.JourneyNumber)
            .Distinct()
            .OrderBy(x => x))
        {
            var rows = calculatedRows
                .Where(x => x.JourneyNumber == journeyNumber)
                .OrderByDescending(x => x.CumulativePoints)
                .ThenBy(x => x.UserId)
                .ToList();

            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].PositionInJourney = i + 1;
            }
        }

        var existingMap = existingRows.ToDictionary(
            x => (x.UserId, x.JourneyDate.Date));

        var calculatedKeys = calculatedRows
            .Select(x => (x.UserId, x.JourneyDate.Date))
            .ToHashSet();

        var rowsToDelete = existingRows
            .Where(x => !calculatedKeys.Contains((x.UserId, x.JourneyDate.Date)))
            .ToList();

        if (rowsToDelete.Count > 0)
        {
            _dbContext.GroupUserJourneyStandingSnapshots.RemoveRange(rowsToDelete);
        }

        foreach (var row in calculatedRows)
        {
            if (existingMap.TryGetValue((row.UserId, row.JourneyDate.Date), out var existing))
            {
                existing.JourneyNumber = row.JourneyNumber;
                existing.PointsOfDay = row.PointsOfDay;
                existing.CumulativePoints = row.CumulativePoints;
                existing.PositionInJourney = row.PositionInJourney;
                existing.LastMatchId = row.LastMatchId;
                existing.UpdatedAtUtc = row.UpdatedAtUtc;
            }
            else
            {
                _dbContext.GroupUserJourneyStandingSnapshots.Add(row);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static int GetOutcome(int homeGoals, int awayGoals)
    {
        if (homeGoals > awayGoals) return 1;
        if (homeGoals < awayGoals) return -1;
        return 0;
    }

    private static MatchCategory GetMatchCategory(int homeGoals, int awayGoals)
    {
        var diff = Math.Abs(homeGoals - awayGoals);

        if (diff <= 1) return MatchCategory.Closed;
        if (diff == 2) return MatchCategory.Comfortable;
        return MatchCategory.Blowout;
    }

    private static int GetCategoryPoints(
        GroupScoringRule rule,
        int homeGoals,
        int awayGoals)
    {
        return GetMatchCategory(homeGoals, awayGoals) switch
        {
            MatchCategory.Closed => rule.ClosedMatchPoints ?? 0,
            MatchCategory.Comfortable => rule.ComfortableWinPoints ?? 0,
            MatchCategory.Blowout => rule.BlowoutPoints ?? 0,
            _ => 0
        };
    }

    private enum MatchCategory
    {
        Closed = 1,
        Comfortable = 2,
        Blowout = 3
    }

    private sealed class MemberInfo
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string? Nickname { get; set; }
        public string? PhotoKey { get; set; }
    }

    private sealed class MatchScoreBreakdown
    {
        public int TotalPoints { get; set; }
        public int OutcomePoints { get; set; }
        public int ExactHomeGoalsPoints { get; set; }
        public int ExactAwayGoalsPoints { get; set; }
        public int CategoryPoints { get; set; }
    }

    private static MatchScoreBreakdown CalculateMatchScoreBreakdown(
        GroupScoringRule rule,
        int predictedHomeGoals,
        int predictedAwayGoals,
        int actualHomeGoals,
        int actualAwayGoals)
    {
        var result = new MatchScoreBreakdown();

        if (rule.EnableOutcomeRule)
        {
            if (GetOutcome(predictedHomeGoals, predictedAwayGoals) ==
                GetOutcome(actualHomeGoals, actualAwayGoals))
            {
                result.OutcomePoints = rule.OutcomePoints ?? 0;
            }
        }

        if (rule.EnableExactScoreRule)
        {
            var homeMatched = predictedHomeGoals == actualHomeGoals;
            var awayMatched = predictedAwayGoals == actualAwayGoals;

            if (rule.RequireBothExactScores)
            {
                if (homeMatched && awayMatched)
                {
                    result.ExactHomeGoalsPoints = rule.ExactHomeGoalsPoints ?? 0;
                    result.ExactAwayGoalsPoints = rule.ExactAwayGoalsPoints ?? 0;
                }
            }
            else
            {
                if (homeMatched)
                    result.ExactHomeGoalsPoints = rule.ExactHomeGoalsPoints ?? 0;

                if (awayMatched)
                    result.ExactAwayGoalsPoints = rule.ExactAwayGoalsPoints ?? 0;
            }
        }

        if (rule.EnableGoalDifferenceRule)
        {
            if (GetMatchCategory(predictedHomeGoals, predictedAwayGoals) ==
                GetMatchCategory(actualHomeGoals, actualAwayGoals))
            {
                result.CategoryPoints = GetCategoryPoints(
                    rule,
                    actualHomeGoals,
                    actualAwayGoals);
            }
        }

        result.TotalPoints =
            result.OutcomePoints +
            result.ExactHomeGoalsPoints +
            result.ExactAwayGoalsPoints +
            result.CategoryPoints;

        return result;
    }

    private async Task<string?> CreatePhotoUrlAsync(
        string? photoKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoKey))
            return null;

        try
        {
            var readUrl = await _storageService.CreateReadUrlAsync(
                photoKey,
                cancellationToken);

            return readUrl.Url;
        }
        catch
        {
            return null;
        }
    }

    private async Task<GroupDashboardResponseDto> GetOfficialDashboardFromSnapshotsAsync(
        int groupId,
        CancellationToken cancellationToken)
    {
        var data = await (
    from snapshot in _dbContext.GroupUserJourneyStandingSnapshots.AsNoTracking()
    join user in _dbContext.Users.AsNoTracking()
        on snapshot.UserId equals user.Id
    where snapshot.GroupId == groupId
    select new
    {
        PlayerId = snapshot.UserId,
        PlayerName = user.Nickname ?? user.Email,
        user.PhotoKey,
        snapshot.JourneyNumber,
        snapshot.JourneyDate,
        snapshot.PointsOfDay,
        snapshot.CumulativePoints,
        snapshot.PositionInJourney
    }
).ToListAsync(cancellationToken);

        if (!data.Any())
        {
            return new GroupDashboardResponseDto
            {
                GroupId = groupId
            };
        }

        var photoUrlMap = new Dictionary<int, string?>();

        foreach (var item in data
            .GroupBy(x => new { x.PlayerId, x.PhotoKey })
            .Select(g => new { g.Key.PlayerId, g.Key.PhotoKey }))
        {
            photoUrlMap[item.PlayerId] = await CreatePhotoUrlAsync(
                item.PhotoKey,
                cancellationToken);
        }

        var players = data
            .GroupBy(x => new { x.PlayerId, x.PlayerName, x.PhotoKey })
            .Select(g => new GroupDashboardPlayerDto
            {
                Id = g.Key.PlayerId,
                Name = g.Key.PlayerName,
                PhotoUrl = photoUrlMap.TryGetValue(g.Key.PlayerId, out var photoUrl)
                    ? photoUrl
                    : null
            })
            .OrderBy(x => x.Name)
            .ToList();

        var journeys = data
            .GroupBy(x => new
            {
                x.JourneyNumber,
                JourneyDate = x.JourneyDate.Date
            })
            .OrderBy(x => x.Key.JourneyNumber)
            .Select(g => new GroupDashboardJourneyDto
            {
                JourneyNumber = g.Key.JourneyNumber,
                JourneyDate = g.Key.JourneyDate.ToString("yyyy-MM-dd"),
                JourneyLabel = g.Key.JourneyDate.ToString("MMM dd")
            })
            .ToList();

        var points = data
            .OrderBy(x => x.PlayerId)
            .ThenBy(x => x.JourneyNumber)
            .Select(x => new GroupDashboardPointDto
            {
                PlayerId = x.PlayerId,
                PlayerName = x.PlayerName,
                PhotoUrl = photoUrlMap.TryGetValue(x.PlayerId, out var photoUrl)
                    ? photoUrl
                    : null,

                JourneyNumber = x.JourneyNumber,
                JourneyDate = x.JourneyDate.ToString("yyyy-MM-dd"),
                JourneyLabel = x.JourneyDate.ToString("MMM dd"),

                PointsOfDay = x.PointsOfDay,
                CumulativePoints = x.CumulativePoints,
                PositionInJourney = x.PositionInJourney
            })
            .ToList();

        return new GroupDashboardResponseDto
        {
            GroupId = groupId,
            Players = players,
            Journeys = journeys,
            Points = points
        };

    }

    private async Task<GroupDashboardResponseDto> CalculateSimulationDashboardOnTheFlyAsync(
        int groupId,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var scoringRule = await _dbContext.GroupScoringRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GroupId == groupId, cancellationToken);

        if (scoringRule is null)
            return new GroupDashboardResponseDto { GroupId = groupId };

        var members = await (
            from gm in _dbContext.GroupMembers.AsNoTracking()
            join u in _dbContext.Users.AsNoTracking() on gm.UserId equals u.Id
            where gm.GroupId == groupId && !gm.IsDeleted && gm.IsEnabled
            select new
            {
                UserId = u.Id,
                PlayerName = u.Nickname ?? u.Email,
                u.PhotoKey
            }
        ).ToListAsync(cancellationToken);

        var photoUrlMap = new Dictionary<int, string?>();

        foreach (var member in members)
        {
            photoUrlMap[member.UserId] = await CreatePhotoUrlAsync(
                member.PhotoKey,
                cancellationToken);
        }

        var memberIds = members.Select(x => x.UserId).ToList();

        var simulations = await _dbContext.UserMatchSimulations
            .AsNoTracking()
            .Where(x =>
                x.UserId == currentUserId &&
                x.HasSimulation &&
                x.SimulatedHomeGoals.HasValue &&
                x.SimulatedAwayGoals.HasValue)
            .ToListAsync(cancellationToken);

        if (!simulations.Any() || !memberIds.Any())
            return new GroupDashboardResponseDto { GroupId = groupId };

        var simulatedMatchIds = simulations.Select(x => x.MatchId).ToList();

        var matches = await _dbContext.Matches
            .AsNoTracking()
            .Where(x =>
                simulatedMatchIds.Contains(x.Id) &&
                x.StageCode == FirstRoundStageCode &&
                x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.MatchDateUtc
            })
            .ToListAsync(cancellationToken);

        if (!matches.Any())
            return new GroupDashboardResponseDto { GroupId = groupId };

        var matchIds = matches.Select(x => x.Id).ToList();

        var predictions = await _dbContext.UserMatchPredictions
            .AsNoTracking()
            .Where(x =>
                memberIds.Contains(x.UserId) &&
                matchIds.Contains(x.MatchId) &&
                x.HasPrediction &&
                x.PredictedHomeGoals.HasValue &&
                x.PredictedAwayGoals.HasValue)
            .ToListAsync(cancellationToken);

        var simulationMap = simulations.ToDictionary(x => x.MatchId);
        var matchMap = matches.ToDictionary(x => x.Id);

        var scoreRows = new List<dynamic>();


        var groupTimeZoneId = await _dbContext.Groups
            .AsNoTracking()
            .Where(x => x.Id == groupId)
            .Select(x => x.TimeZoneId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? "America/New_York";

        foreach (var prediction in predictions)
        {
            if (!simulationMap.TryGetValue(prediction.MatchId, out var sim))
                continue;

            if (!matchMap.TryGetValue(prediction.MatchId, out var match))
                continue;

            var breakdown = CalculateMatchScoreBreakdown(
                scoringRule,
                prediction.PredictedHomeGoals!.Value,
                prediction.PredictedAwayGoals!.Value,
                sim.SimulatedHomeGoals!.Value,
                sim.SimulatedAwayGoals!.Value);

            scoreRows.Add(new
            {
                prediction.UserId,
                JourneyDate = GetGroupLocalDate(
                    match.MatchDateUtc,
                    groupTimeZoneId),
                Points = breakdown.TotalPoints,
                MatchId = prediction.MatchId
            });
        }


        if (!scoreRows.Any())
            return new GroupDashboardResponseDto { GroupId = groupId };

        var dailyData = scoreRows
            .GroupBy(x => new { x.UserId, x.JourneyDate })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.JourneyDate,
                PointsOfDay = g.Sum(x => x.Points),
                LastMatchId = g.Max(x => x.MatchId)
            })
            .ToList();

        var journeyDates = dailyData
            .Select(x => x.JourneyDate)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var journeys = journeyDates
            .Select((date, index) => new GroupDashboardJourneyDto
            {
                JourneyNumber = index + 1,
                JourneyDate = date.ToString("yyyy-MM-dd"),
                JourneyLabel = date.ToString("MMM dd")
            })
            .ToList();

        var journeyMap = journeys.ToDictionary(
            x => DateTime.Parse(x.JourneyDate).Date,
            x => x.JourneyNumber);

        var players = members
            .Select(x => new GroupDashboardPlayerDto
            {
                Id = x.UserId,
                Name = x.PlayerName,
                PhotoUrl = photoUrlMap.TryGetValue(x.UserId, out var photoUrl)
                    ? photoUrl
                    : null
            })
            .OrderBy(x => x.Name)
            .ToList();

        var pointsMap = dailyData.ToDictionary(
            x => (x.UserId, x.JourneyDate),
            x => x.PointsOfDay);

        var points = new List<GroupDashboardPointDto>();

        var cumulativeMap = members.ToDictionary(x => x.UserId, x => 0);

        foreach (var journeyDate in journeyDates)
        {
            var journeyNumber = journeyMap[journeyDate];

            var rowsForJourney = new List<GroupDashboardPointDto>();

            foreach (var member in members)
            {
                pointsMap.TryGetValue((member.UserId, journeyDate), out var pointsOfDay);

                cumulativeMap[member.UserId] += pointsOfDay;

                rowsForJourney.Add(new GroupDashboardPointDto
                {
                    PlayerId = member.UserId,
                    PlayerName = member.PlayerName,
                    PhotoUrl = photoUrlMap.TryGetValue(member.UserId, out var photoUrl)
                    ? photoUrl
                    : null,
                    JourneyNumber = journeyNumber,
                    JourneyDate = journeyDate.ToString("yyyy-MM-dd"),
                    JourneyLabel = journeyDate.ToString("MMM dd"),
                    PointsOfDay = pointsOfDay,
                    CumulativePoints = cumulativeMap[member.UserId]
                });
            }

            var ordered = rowsForJourney
                .OrderByDescending(x => x.CumulativePoints)
                .ThenBy(x => x.PlayerName)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
                ordered[i].PositionInJourney = i + 1;

            points.AddRange(ordered);
        }

        return new GroupDashboardResponseDto
        {
            GroupId = groupId,
            Players = players,
            Journeys = journeys,
            Points = points
        };
    }

    public async Task<GroupDashboardResponseDto?> GetGroupDashboardAsync(
        int groupId,
        string firebaseUid,
        GroupStandingsMode mode,
        CancellationToken cancellationToken)
    {
        var currentUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (currentUser is null) return null;

        var isMember = await _dbContext.GroupMembers
            .AsNoTracking()
            .AnyAsync(x =>
                x.GroupId == groupId &&
                x.UserId == currentUser.Id &&
                !x.IsDeleted &&
                x.IsEnabled,
                cancellationToken);

        if (!isMember) return null;

        if (mode == GroupStandingsMode.Simulation)
        {
            return await CalculateSimulationDashboardOnTheFlyAsync(
                groupId,
                currentUser.Id,
                cancellationToken);
        }

        return await GetOfficialDashboardFromSnapshotsAsync(
            groupId,
            cancellationToken);

    }

    private static DateTime GetGroupLocalDate(DateTime matchDateUtc, string timeZoneId)
    {
        var utc = DateTime.SpecifyKind(matchDateUtc, DateTimeKind.Utc);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone).Date;
    }

}