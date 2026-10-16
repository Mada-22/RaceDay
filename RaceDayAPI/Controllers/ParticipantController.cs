using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Services;
using RaceDayAPI.DTOs.Profiles;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Participant")]
    public class ParticipantController : ControllerBase
    {
        private readonly ParticipantService _participantService;


    public ParticipantController(ParticipantService participantService)
        {
            _participantService = participantService;
        }

        // GET: api/Participant/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var accountId = GetAccountId();

                var profile = await _participantService
                    .GetProfileAsync(accountId);

                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // PUT: api/Participant/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateParticipantProfileDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var updatedProfile = await _participantService
                    .UpdateProfileAsync(accountId, dto);

                return Ok(updatedProfile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetAccountId()
        {
            var accountIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(accountIdValue, out var accountId))
            {
                throw new UnauthorizedAccessException(
                    "The authenticated account ID is invalid.");
            }

            return accountId;
        }
    }


}

