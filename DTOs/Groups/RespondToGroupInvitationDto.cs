using System.ComponentModel.DataAnnotations;

namespace Goal2026API.Api.DTOs.Groups;

public sealed class RespondToGroupInvitationDto
{
    [Required]
    public string Token { get; set; } = null!;
}