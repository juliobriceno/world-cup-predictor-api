using System.ComponentModel.DataAnnotations;

namespace Goal2026API.Api.DTOs.Groups;

public sealed class RespondToGroupInvitationByIdDto
{
    [Required]
    public int InvitationId { get; set; }
}