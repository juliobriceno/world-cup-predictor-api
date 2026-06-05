namespace Goal2026API.Api.Entities;

public sealed class GroupUserJourneyStandingSnapshot
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public int UserId { get; set; }

    public int JourneyNumber { get; set; }
    public DateTime JourneyDate { get; set; }

    public int PointsOfDay { get; set; }
    public int CumulativePoints { get; set; }
    public int PositionInJourney { get; set; }

    public int? LastMatchId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}