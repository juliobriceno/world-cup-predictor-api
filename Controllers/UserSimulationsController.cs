using Goal2026API.Common;
using Goal2026API.DTOs.UserSimulationsDto;
using Goal2026API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Goal2026API.Controllers
{
    [ApiController]
    [Route("api/user-simulations")]
    [Authorize]
    public sealed class UserSimulationsController : ControllerBase
    {
        private readonly IUserSimulationService _service;

        public UserSimulationsController(IUserSimulationService service)
        {
            _service = service;
        }

        private string FirebaseUid => User.GetFirebaseUid();

        [HttpGet("me")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var result = await _service.GetMySimulationsAsync(FirebaseUid, ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPut("me")]
        public async Task<IActionResult> Save(
            [FromBody] SaveUserSimulationsDto dto,
            CancellationToken ct)
        {
            var result = await _service.SaveMySimulationsAsync(FirebaseUid, dto, ct);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
