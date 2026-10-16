using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RaceDayAPI.DTOs.Categories;
using RaceDayAPI.Service;

using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;


    public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Category/event/1
        // Authenticated organizers and participants can view event categories.
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            try
            {
                var categories = await _categoryService
                    .GetByEventAsync(eventId);

                return Ok(categories);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Category/event/1
        // Only organizers can create a category for their event.
        [HttpPost("event/{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Create(
            int eventId,
            [FromBody] CreateCategoryDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var category = await _categoryService.CreateAsync(
                    accountId, eventId, dto);

                return Ok(category);
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

        // PUT: api/Category/1
        
[HttpPut("event/{eventId:int}/category/{categoryId:int}")]
[Authorize(Roles = "Organizer")]
public async Task<IActionResult> Update(
    int eventId,
    int categoryId,
    [FromBody] UpdateCategoryDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var category = await _categoryService.UpdateAsync(
                    accountId,
                    eventId,
                    categoryId,
                    dto);

                if (category == null)
                {
                    return NotFound(new
                    {
                        message = "Event or category not found."
                    });
                }

                return Ok(category);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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



        // DELETE: api/Category/1
        [HttpDelete("event/{eventId:int}/category/{categoryId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int eventId, int categoryId)
        {
            try
            {
                var accountId = GetAccountId();


    var deleted = await _categoryService.DeleteAsync(
        accountId,
        eventId,
        categoryId);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message = "Event or category assignment not found."
                    });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
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

