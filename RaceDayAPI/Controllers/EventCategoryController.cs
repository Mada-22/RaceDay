using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventCategoryController : ControllerBase
    {
        private readonly EventCategoryService _eventCategoryService;


    public EventCategoryController(
        EventCategoryService eventCategoryService)
        {
            _eventCategoryService = eventCategoryService;
        }

        // GET: api/EventCategory/event/1
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            try
            {
                var accountId = GetAccountId();

                var categories = await _eventCategoryService
                    .GetByEventAsync(accountId, eventId);

                return Ok(categories);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/EventCategory/event/1/category/2
        [HttpPost("event/{eventId:int}/category/{categoryId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> AssignCategory(
            int eventId,
            int categoryId)
        {
            try
            {
                var accountId = GetAccountId();

                await _eventCategoryService.AssignCategoryAsync(
                    accountId,
                    eventId,
                    categoryId);

                return Ok(new
                {
                    message = "Category assigned to event successfully."
                });
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

        // DELETE: api/EventCategory/event/1/category/2
        [HttpDelete("event/{eventId:int}/category/{categoryId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> RemoveCategory(
            int eventId,
            int categoryId)
        {
            try
            {
                var accountId = GetAccountId();

                await _eventCategoryService.RemoveCategoryAsync(
                    accountId,
                    eventId,
                    categoryId);

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

