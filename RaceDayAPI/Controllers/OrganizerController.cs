using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RaceDayAPI.DTOs.Profiles;
using RaceDayAPI.Service;

using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : ControllerBase
    {
        private readonly OrganizerService _organizerService;


    public OrganizerController(OrganizerService organizerService)
        {
            _organizerService = organizerService;
        }

        // GET: api/Organizer/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var accountId = GetAccountId();

                var profile = await _organizerService
                    .GetProfileAsync(accountId);

                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/Organizer/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateOrganizerProfileDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var updatedProfile = await _organizerService
                    .UpdateProfileAsync(accountId, dto);

                return Ok(updatedProfile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

