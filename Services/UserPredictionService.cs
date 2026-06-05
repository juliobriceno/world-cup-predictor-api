using Goal2026API.Api.Data;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Goal2026API.Api.Data.Entities;

namespace Goal2026API.Api.Services;

public sealed class UserPredictionService : IUserPredictionService
{
    private const string FirstRoundStageCode = "GROUP";

    private readonly AppDbContext _dbContext;
    private const string WorldCupStartedKey = "WorldCupStarted";

    public UserPredictionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private async Task<bool> HasWorldCupStartedAsync(CancellationToken cancellationToken)
    {
        var value = await _dbContext.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == WorldCupStartedKey)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return bool.TryParse(value, out var parsed) && parsed;
    }

    public async Task<SaveUserPredictionsResponseDto?> SaveMyPredictionsAsync(
        string firebaseUid,
        SaveUserPredictionsDto dto,
        CancellationToken cancellationToken)
    {
        dto ??= new SaveUserPredictionsDto();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var worldCupStarted = await HasWorldCupStartedAsync(cancellationToken);

        if (worldCupStarted)
        {
            throw new InvalidOperationException("Predictions can no longer be modified.");
        }

        var officialMatches = await _dbContext.Matches
            .AsNoTracking()
            .Where(x => x.StageCode == FirstRoundStageCode && x.IsActive)
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id })
            .ToListAsync(cancellationToken);

        var officialMatchIds = officialMatches
            .Select(x => x.Id)
            .ToList();

        var officialMatchIdSet = officialMatchIds.ToHashSet();

        var incomingItems = (dto.Matches ?? new List<UserPredictionItemDto>())
            .Where(x => officialMatchIdSet.Contains(x.MatchId))
            .GroupBy(x => x.MatchId)
            .Select(x => x.Last())
            .ToDictionary(x => x.MatchId, x => x);

        var existingRows = await _dbContext.UserMatchPredictions
            .Where(x => x.UserId == user.Id && officialMatchIdSet.Contains(x.MatchId))
            .ToListAsync(cancellationToken);

        var existingMap = existingRows.ToDictionary(x => x.MatchId, x => x);

        var utcNow = DateTime.UtcNow;

        foreach (var matchId in officialMatchIds)
        {
            incomingItems.TryGetValue(matchId, out var incomingItem);

            var hasPrediction =
                incomingItem is not null &&
                incomingItem.PredictedHomeGoals.HasValue &&
                incomingItem.PredictedAwayGoals.HasValue;

            int? predictedHomeGoals = hasPrediction
                ? incomingItem!.PredictedHomeGoals
                : null;

            int? predictedAwayGoals = hasPrediction
                ? incomingItem!.PredictedAwayGoals
                : null;

            if (existingMap.TryGetValue(matchId, out var existingRow))
            {
                existingRow.PredictedHomeGoals = predictedHomeGoals;
                existingRow.PredictedAwayGoals = predictedAwayGoals;
                existingRow.HasPrediction = hasPrediction;
                existingRow.UpdatedAtUtc = utcNow;
            }
            else
            {
                var newRow = new UserMatchPrediction
                {
                    UserId = user.Id,
                    MatchId = matchId,
                    PredictedHomeGoals = predictedHomeGoals,
                    PredictedAwayGoals = predictedAwayGoals,
                    HasPrediction = hasPrediction,
                    CreatedAtUtc = utcNow,
                    UpdatedAtUtc = utcNow
                };

                _dbContext.UserMatchPredictions.Add(newRow);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var predictedMatches = officialMatchIds.Count(matchId =>
        {
            if (!incomingItems.TryGetValue(matchId, out var item))
            {
                return false;
            }

            return item.PredictedHomeGoals.HasValue && item.PredictedAwayGoals.HasValue;
        });

        return new SaveUserPredictionsResponseDto
        {
            Message = "Predictions saved successfully.",
            TotalMatches = officialMatchIds.Count,
            PredictedMatches = predictedMatches,
            PendingMatches = officialMatchIds.Count - predictedMatches,
            SavedAtUtc = utcNow
        };
    }

    public async Task<GetUserPredictionsResponseDto?> GetMyPredictionsAsync(
        string firebaseUid,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var officialMatchIds = await _dbContext.Matches
            .AsNoTracking()
            .Where(x => x.StageCode == FirstRoundStageCode && x.IsActive)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var predictions = await _dbContext.UserMatchPredictions
            .AsNoTracking()
            .Where(x => x.UserId == user.Id && officialMatchIds.Contains(x.MatchId))
            .OrderBy(x => x.MatchId)
            .Select(x => new UserPredictionItemResponseDto
            {
                MatchId = x.MatchId,
                PredictedHomeGoals = x.PredictedHomeGoals,
                PredictedAwayGoals = x.PredictedAwayGoals,
                HasPrediction = x.HasPrediction
            })
            .ToListAsync(cancellationToken);

        var predictedMatches = predictions.Count(x =>
            x.HasPrediction &&
            x.PredictedHomeGoals.HasValue &&
            x.PredictedAwayGoals.HasValue);

        return new GetUserPredictionsResponseDto
        {
            TotalMatches = officialMatchIds.Count,
            PredictedMatches = predictedMatches,
            PendingMatches = officialMatchIds.Count - predictedMatches,
            Matches = predictions
        };
    }

}