namespace Goal2026API.Api.DTOs;

public sealed class GroupStandingsResponseDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public GroupStandingsMode Mode { get; set; }
    public bool WorldCupStarted { get; set; }
    public int EvaluatedMatches { get; set; }
    public List<GroupStandingRowDto> Standings { get; set; } = new();
}