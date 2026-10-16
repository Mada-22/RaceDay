using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Routes;
using RaceDayAPI.Model;
namespace RaceDayAPI.Service
{
    public class RouteService
    {
        private readonly RaceDayDBContext _context;

        public RouteService(RaceDayDBContext context)
        {
            _context = context;
        }

        // Retrieve all routes belonging to an event
        public async Task<List<RouteResponseDTO>> GetByEventAsync(
            int eventId)
        {
            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
            {
                throw new KeyNotFoundException("Event not found.");
            }

            var routes = await _context.Routes
                .Where(r => r.EventID == eventId)
                .ToListAsync();

            return routes.Select(MapToResponse).ToList();
        }

        // Retrieve a specific route
        public async Task<RouteResponseDTO?> GetByIdAsync(
            int eventId,
            int routeId)
        {
            var route = await _context.Routes
                .SingleOrDefaultAsync(r =>
                    r.EventID == eventId &&
                    r.RouteID == routeId);

            if (route == null)
            {
                return null;
            }

            return MapToResponse(route);
        }

        // Create a route for an organizer's event
        public async Task<RouteResponseDTO> CreateAsync(
            int accountId,
            int eventId,
            CreateRouteDTO dto)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .SingleOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null)
            {
                throw new KeyNotFoundException("Event not found.");
            }

            if (eventEntity.Organizer.AccountID != accountId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot create routes for another organizer's event.");
            }

            ValidateRoute(dto.RouteName, dto.Difficulty, dto.Distance);

            var route = new Route_
            {
                EventID = eventId,
                RouteName = dto.RouteName.Trim(),
                Difficulty = dto.Difficulty.Trim(),
                Distance = dto.Distance
            };

            _context.Routes.Add(route);

            await _context.SaveChangesAsync();

            return MapToResponse(route);
        }

        // Update an existing route
        public async Task<RouteResponseDTO?> UpdateAsync(
            int accountId,
            int eventId,
            int routeId,
            UpdateRouteDTO dto)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .SingleOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null)
            {
                return null;
            }

            if (eventEntity.Organizer.AccountID != accountId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update routes for another organizer's event.");
            }

            var route = await _context.Routes
                .SingleOrDefaultAsync(r =>
                    r.EventID == eventId &&
                    r.RouteID == routeId);

            if (route == null)
            {
                return null;
            }

            ValidateRoute(dto.RouteName, dto.Difficulty, dto.Distance);

            route.RouteName = dto.RouteName.Trim();
            route.Difficulty = dto.Difficulty.Trim();
            route.Distance = dto.Distance;

            await _context.SaveChangesAsync();

            return MapToResponse(route);
        }

        // Delete a route
        public async Task<bool> DeleteAsync(
            int accountId,
            int eventId,
            int routeId)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .SingleOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null)
            {
                return false;
            }

            if (eventEntity.Organizer.AccountID != accountId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot delete routes for another organizer's event.");
            }

            var route = await _context.Routes
                .SingleOrDefaultAsync(r =>
                    r.EventID == eventId &&
                    r.RouteID == routeId);

            if (route == null)
            {
                return false;
            }

            bool routeInUse = await _context.Entries
                .AnyAsync(entry => entry.RouteID == routeId);

            if (routeInUse)
            {
                throw new InvalidOperationException(
                    "This route cannot be deleted because participants have registered for it.");
            }

            _context.Routes.Remove(route);

            await _context.SaveChangesAsync();

            return true;
        }

        // Validate route information
        private static void ValidateRoute(
            string routeName,
            string difficulty,
            decimal distance)
        {
            if (string.IsNullOrWhiteSpace(routeName))
            {
                throw new ArgumentException(
                    "Route name is required.");
            }

            if (string.IsNullOrWhiteSpace(difficulty))
            {
                throw new ArgumentException(
                    "Route difficulty is required.");
            }

            if (distance <= 0)
            {
                throw new ArgumentException(
                    "Route distance must be greater than zero.");
            }

            string[] allowedDifficulties =
            {
            "Easy",
            "Moderate",
            "Hard"
        };

            if (!allowedDifficulties.Contains(
                difficulty.Trim(),
                StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Difficulty must be Easy, Moderate, or Hard.");
            }
        }

        // Map route entity to response DTO
        private static RouteResponseDTO MapToResponse(Route_ route)
        {
            return new RouteResponseDTO
            {
                RouteID = route.RouteID,
                EventID = route.EventID,
                RouteName = route.RouteName,
                Difficulty = route.Difficulty,
                Distance = route.Distance
            };
        }
    }
}
