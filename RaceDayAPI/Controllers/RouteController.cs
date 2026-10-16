using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.DTOs;
using RaceDayAPI.DTOs.Routes;
using RaceDayAPI.Service;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RouteController : ControllerBase
    {
        private readonly RouteService _routeService;


    public RouteController(RouteService routeService)
        {
            _routeService = routeService;
        }

        // GET: api/Route/event/1
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            try
            {
                var routes = await _routeService.GetByEventAsync(eventId);

                return Ok(routes);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Route/1
       
       
[HttpGet("event/{eventId:int}/route/{routeId:int}")]
public async Task<IActionResult> GetById(int eventId, int routeId)
        {
            var route = await _routeService.GetByIdAsync(eventId, routeId);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Event or route not found."
                });
            }

            return Ok(route);
        }



        // POST: api/Route/event/1
        [HttpPost("event/{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Create(
            int eventId,
            [FromBody] CreateRouteDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var route = await _routeService.CreateAsync(
                    accountId, eventId, dto);

                return Ok(route);
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

        // PUT: api/Route/1
      
[HttpPut("event/{eventId:int}/route/{routeId:int}")]
[Authorize(Roles = "Organizer")]
public async Task<IActionResult> Update(
    int eventId,
    int routeId,
    [FromBody] UpdateRouteDTO dto)
        {
            try
            {
                var accountId = GetAccountId();

                var route = await _routeService.UpdateAsync(
                    accountId,
                    eventId,
                    routeId,
                    dto);

                if (route == null)
                {
                    return NotFound(new
                    {
                        message = "Event or route not found."
                    });
                }

                return Ok(route);
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


        // DELETE: api/Route/1
        [HttpDelete("event/{eventId:int}/route/{routeId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int eventId, int routeId)
        {
            try
            {
                var accountId = GetAccountId();


    var deleted = await _routeService.DeleteAsync(
        accountId,
        eventId,
        routeId);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message = "Event or route not found."
                    });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
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
