using Goal2026API.Api.Data;
using Goal2026API.Api.Data.Entities;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Entities;
using Goal2026API.Api.Services;
using Goal2026API.DTOs.UserSimulationsDto;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Services
{
    public sealed class UserSimulationService : IUserSimulationService
    {
        private const string FirstRoundStageCode = "GROUP";
        private readonly AppDbContext _dbContext;
        private readonly ApiNotificationService _notificationService;

        public UserSimulationService(AppDbContext dbContext, ApiNotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<SaveUserSimulationsResponseDto?> SaveMySimulationsAsync(
            string firebaseUid,
            SaveUserSimulationsDto dto,
            CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

            if (user is null) return null;

            var matches = await _dbContext.Matches
                .Where(x => x.StageCode == FirstRoundStageCode && x.IsActive)
                .ToListAsync(cancellationToken);

            var openMatches = matches
                .Where(x => !x.IsFinished)
                .Select(x => x.Id)
                .ToHashSet();

            var incoming = dto.Matches
                .Where(x => openMatches.Contains(x.MatchId))
                .GroupBy(x => x.MatchId)
                .Select(x => x.Last())
                .ToDictionary(x => x.MatchId, x => x);

            var existing = await _dbContext.UserMatchSimulations
                .Where(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken);

            var map = existing.ToDictionary(x => x.MatchId);

            var now = DateTime.UtcNow;

            foreach (var match in matches)
            {
                if (match.IsFinished)
                    continue; // 🔥 NO permitir simulación en partidos cerrados

                incoming.TryGetValue(match.Id, out var item);

                var hasSimulation =
                    item != null &&
                    item.SimulatedHomeGoals.HasValue &&
                    item.SimulatedAwayGoals.HasValue;

                if (map.TryGetValue(match.Id, out var row))
                {
                    row.SimulatedHomeGoals = hasSimulation ? item!.SimulatedHomeGoals : null;
                    row.SimulatedAwayGoals = hasSimulation ? item!.SimulatedAwayGoals : null;
                    row.HasSimulation = hasSimulation;
                    row.UpdatedAtUtc = now;
                }
                else
                {
                    _dbContext.UserMatchSimulations.Add(new UserMatchSimulation
                    {
                        UserId = user.Id,
                        MatchId = match.Id,
                        SimulatedHomeGoals = hasSimulation ? item!.SimulatedHomeGoals : null,
                        SimulatedAwayGoals = hasSimulation ? item!.SimulatedAwayGoals : null,
                        HasSimulation = hasSimulation,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var simulatedCount = incoming.Count(x =>
                x.Value.SimulatedHomeGoals.HasValue &&
                x.Value.SimulatedAwayGoals.HasValue);

            await _notificationService.CreateAsync(
                user.Id,
                "simulation-saved",
                "Simulations saved",
                $"You have saved {simulatedCount} simulations.",
                new List<string> { "Push" },
                $"simulation-saved:{user.Id}:{DateTime.UtcNow:yyyy-MM-dd}",
                cancellationToken);

            return new SaveUserSimulationsResponseDto
            {
                Message = "Simulations saved.",
                TotalMatches = matches.Count,
                SimulatedMatches = simulatedCount,
                PendingMatches = matches.Count - simulatedCount,
                SavedAtUtc = now
            };
        }

        public async Task<GetUserSimulationsResponseDto?> GetMySimulationsAsync(
            string firebaseUid,
            CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

            if (user is null) return null;

            var matches = await _dbContext.Matches
                .Where(x => x.StageCode == FirstRoundStageCode && x.IsActive)
                .ToListAsync(cancellationToken);

            var sims = await _dbContext.UserMatchSimulations
                .Where(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken);

            var map = sims.ToDictionary(x => x.MatchId);

            var result = matches.Select(m =>
            {
                map.TryGetValue(m.Id, out var sim);

                return new UserSimulationItemResponseDto
                {
                    MatchId = m.Id,
                    SimulatedHomeGoals = sim?.SimulatedHomeGoals,
                    SimulatedAwayGoals = sim?.SimulatedAwayGoals,
                    HasSimulation = sim?.HasSimulation ?? false
                };
            }).ToList();

            var simulatedCount = result.Count(x => x.HasSimulation);

            return new GetUserSimulationsResponseDto
            {
                TotalMatches = matches.Count,
                SimulatedMatches = simulatedCount,
                PendingMatches = matches.Count - simulatedCount,
                Matches = result
            };
        }
    }
}
