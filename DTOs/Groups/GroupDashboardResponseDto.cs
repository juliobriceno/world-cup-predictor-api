namespace Goal2026API.Api.DTOs.Groups;

public class GroupDashboardResponseDto
{
    public int GroupId { get; set; }
    public List<GroupDashboardPlayerDto> Players { get; set; } = new();
    public List<GroupDashboardJourneyDto> Journeys { get; set; } = new();
    public List<GroupDashboardPointDto> Points { get; set; } = new();
}

public class GroupDashboardPlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? PhotoUrl { get; set; }
}

public class GroupDashboardJourneyDto
{
    public int JourneyNumber { get; set; }
    public string JourneyDate { get; set; } = "";
    public string JourneyLabel { get; set; } = "";
}

public class GroupDashboardPointDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public string? PhotoUrl { get; set; }

    public int JourneyNumber { get; set; }
    public string JourneyDate { get; set; } = "";
    public string JourneyLabel { get; set; } = "";

    public int PointsOfDay { get; set; }
    public int CumulativePoints { get; set; }
    public int PositionInJourney { get; set; }
}