using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.DTOs;
using RaceDayAPI.DTOs.Events;
using RaceDayAPI.Service;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;


    public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        // GET: api/Event
        // Any authenticated user can view events.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var events = await _eventService.GetAllAsync();

            return Ok(events);
        }

        // GET: api/Event/5
        [HttpGet("{eventId:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int eventId)
        {
            try
            {
                var eventItem = await _eventService.GetByIdAsync(eventId);

                return Ok(eventItem);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Event
        // Only organizers can create events.
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEventDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var createdEvent = await _eventService.CreateAsync(
                    accountId, dto);

                return Ok(createdEvent);
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

        // PUT: api/Event/5
        [HttpPut("{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Update(
            int eventId,
            [FromBody] UpdateEventDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var updatedEvent = await _eventService.UpdateAsync(
                    accountId, eventId, dto);

                return Ok(updatedEvent);
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

        // DELETE: api/Event/5
        [HttpDelete("{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int eventId)
        {
            try
            {
                var accountId = GetAccountId();

                await _eventService.DeleteAsync(accountId, eventId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
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

