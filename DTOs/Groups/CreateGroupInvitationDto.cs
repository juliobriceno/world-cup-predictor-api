using System.ComponentModel.DataAnnotations;

namespace Goal2026API.Api.DTOs.Groups;

public sealed class CreateGroupInvitationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}