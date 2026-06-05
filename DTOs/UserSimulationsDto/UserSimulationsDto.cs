
namespace Goal2026API.DTOs.UserSimulationsDto
{
    public sealed class SaveUserSimulationsDto
    {
        public List<UserSimulationItemDto> Matches { get; set; } = new();
    }

    public sealed class UserSimulationItemDto
    {
        public int MatchId { get; set; }
        public int? SimulatedHomeGoals { get; set; }
        public int? SimulatedAwayGoals { get; set; }
    }

    public sealed class SaveUserSimulationsResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int TotalMatches { get; set; }
        public int SimulatedMatches { get; set; }
        public int PendingMatches { get; set; }
        public DateTime SavedAtUtc { get; set; }
    }

    public sealed class UserSimulationItemResponseDto
    {
        public int MatchId { get; set; }
        public int? SimulatedHomeGoals { get; set; }
        public int? SimulatedAwayGoals { get; set; }
        public bool HasSimulation { get; set; }
    }

    public sealed class GetUserSimulationsResponseDto
    {
        public int TotalMatches { get; set; }
        public int SimulatedMatches { get; set; }
        public int PendingMatches { get; set; }
        public List<UserSimulationItemResponseDto> Matches { get; set; } = new();
    }

}
