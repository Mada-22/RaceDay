
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.DTOs;
using RaceDayAPI.DTOs.Entries;
using RaceDayAPI.Service;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EntryController : ControllerBase
    {
        private readonly EntryService _entryService;

        public EntryController(EntryService entryService)
        {
            _entryService = entryService;
        }

        // Participant: register for an event
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Create(CreateEntryDTO dto)
        {
            try
            {
                int accountId = GetAccountId();

                var entry = await _entryService.CreateAsync(accountId, dto);

                return CreatedAtAction(
                    nameof(GetMyEntries),
                    new { },
                    entry
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // Participant: view their own registrations
        [HttpGet("my-entries")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEntries()
        {
            try
            {
                int accountId = GetAccountId();

                var entries = await _entryService.GetMyEntriesAsync(accountId);

                return Ok(entries);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Organizer: view registrations for an event
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            try
            {
                int accountId = GetAccountId();

                var entries = await _entryService.GetByEventAsync(
                    accountId,
                    eventId
                );

                return Ok(entries);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private int GetAccountId()
        {
            var accountIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(accountIdValue, out int accountId))
            {
                throw new UnauthorizedAccessException(
                    "The authenticated account ID is invalid."
                );
            }

            return accountId;
        }
    }
}


