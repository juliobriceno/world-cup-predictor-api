using Goal2026API.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Controllers
{

    [ApiController]
    [Route("api/matches")]
    [Authorize]
    public sealed class MatchesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MatchesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetMatchesStatus(CancellationToken ct)
        {
            var result = await _dbContext.Matches
                .AsNoTracking()
                .Where(x => x.StageCode == "GROUP" && x.IsActive)
                .Select(x => new
                {
                    matchId = x.Id,
                    isFinished = x.IsFinished,
                    homeTeamGoals = x.HomeTeamGoals,
                    awayTeamGoals = x.AwayTeamGoals
                })
                .ToListAsync(ct);

            return Ok(result);
        }
    }

}
