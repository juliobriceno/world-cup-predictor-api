using Goal2026API.Common;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Goal2026API.Api.Controllers;

[ApiController]
[Route("api/user-predictions")]
[Authorize]
public sealed class UserPredictionsController : ControllerBase
{
    private readonly IUserPredictionService _userPredictionService;

    public UserPredictionsController(IUserPredictionService userPredictionService)
    {
        _userPredictionService = userPredictionService;
    }

    private string FirebaseUid => User.GetFirebaseUid();

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPredictions(CancellationToken cancellationToken)
    {
        var result = await _userPredictionService.GetMyPredictionsAsync(
            FirebaseUid,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> SaveMyPredictions(
        [FromBody] SaveUserPredictionsDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        var result = await _userPredictionService.SaveMyPredictionsAsync(
            FirebaseUid,
            dto,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(result);
    }
}