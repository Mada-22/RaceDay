
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Entries;
using RaceDayAPI.Model;

namespace RaceDayAPI.Service
{
    public class EntryService
    {
        private readonly RaceDayDBContext _context;

        public EntryService(RaceDayDBContext context)
        {
            _context = context;
        }

        // Register the authenticated participant for an event.
        public async Task<EntryResponseDTO> CreateAsync(
            int accountId,
            CreateEntryDTO dto)
        {
            var participant = await _context.Participants
                .SingleOrDefaultAsync(p => p.AccountID == accountId);

            if (participant == null)
            {
                throw new KeyNotFoundException(
                    "Participant profile not found.");
            }

            var eventEntity = await _context.Events
                .SingleOrDefaultAsync(e => e.EventID == dto.EventID);

            if (eventEntity == null)
            {
                throw new KeyNotFoundException("Event not found.");
            }

            // Validate the registration period.
            DateTime today = DateTime.Today;

            if (today < eventEntity.OpenDate.Date ||
                today > eventEntity.ClosingDate.Date)
            {
                throw new InvalidOperationException(
                    "Registration for this event is currently closed.");
            }

            if (today > eventEntity.EventDate.Date)
            {
                throw new InvalidOperationException(
                    "You cannot register for an event that has already taken place.");
            }

            // Prevent duplicate registrations.
            bool alreadyRegistered = await _context.Entries
                .AnyAsync(e =>
                    e.EventID == dto.EventID &&
                    e.ParticipantID == participant.ParticipantID);

            if (alreadyRegistered)
            {
                throw new InvalidOperationException(
                    "You are already registered for this event.");
            }

            // Ensure the category belongs to the selected event.
            bool categoryAvailable = await _context.EventCategories
                .AnyAsync(ec =>
                    ec.EventID == dto.EventID &&
                    ec.CategoryID == dto.CategoryID);

            if (!categoryAvailable)
            {
                throw new ArgumentException(
                    "The selected category is not available for this event.");
            }

            var category = await _context.Categories
                .SingleOrDefaultAsync(c =>
                    c.CategoryID == dto.CategoryID);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            // Validate age only when the category defines age restrictions.
            if (category.MinimumAge.HasValue &&
                participant.Age < category.MinimumAge.Value)
            {
                throw new InvalidOperationException(
                    $"You do not meet the minimum age for the {category.CategoryName} category.");
            }

            if (category.MaximumAge.HasValue &&
                participant.Age > category.MaximumAge.Value)
            {
                throw new InvalidOperationException(
                    $"You exceed the maximum age for the {category.CategoryName} category.");
            }

            // Validate an optional route.
            if (dto.RouteID.HasValue)
            {
                var route = await _context.Routes
                    .SingleOrDefaultAsync(r =>
                        r.RouteID == dto.RouteID.Value &&
                        r.EventID == dto.EventID);

                if (route == null)
                {
                    throw new ArgumentException(
                        "The selected route does not belong to this event.");
                }

                // If the category specifies a distance, ensure the route matches.
                if (category.Distance.HasValue &&
                    route.Distance != category.Distance.Value)
                {
                    throw new InvalidOperationException(
                        "The selected route does not match the category distance.");
                }
            }

            if (dto.Price < 0)
            {
                throw new ArgumentException(
                    "Entry price cannot be negative.");
            }

            var entry = new Entry
            {
                EventID = dto.EventID,
                ParticipantID = participant.ParticipantID,
                CategoryID = dto.CategoryID,
                RouteID = dto.RouteID,
                Price = dto.Price
            };

            _context.Entries.Add(entry);

            await _context.SaveChangesAsync();

            // Load the related data needed for the response.
            var createdEntry = await _context.Entries
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Include(e => e.Route)
                .SingleAsync(e => e.EntryID == entry.EntryID);

            return MapToResponse(createdEntry);
        }

        // Retrieve entries belonging to the authenticated participant.
        public async Task<List<EntryResponseDTO>> GetMyEntriesAsync(
            int accountId)
        {
            var participant = await _context.Participants
                .SingleOrDefaultAsync(p => p.AccountID == accountId);

            if (participant == null)
            {
                throw new KeyNotFoundException(
                    "Participant profile not found.");
            }

            var entries = await _context.Entries
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Include(e => e.Route)
                .ToListAsync();

            return entries
                .Select(MapToResponse)
                .ToList();
        }

        // Allow an organizer to view entries for their own event.
        public async Task<List<EntryResponseDTO>> GetByEventAsync(
            int accountId,
            int eventId)
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
                    "You cannot view entries for another organizer's event.");
            }

            var entries = await _context.Entries
                .Where(e => e.EventID == eventId)
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Include(e => e.Route)
                .ToListAsync();

            return entries
                .Select(MapToResponse)
                .ToList();
        }

        // Map an entry entity to its response DTO.
        private static EntryResponseDTO MapToResponse(Entry entry)
        {
            return new EntryResponseDTO
            {
                EntryID = entry.EntryID,
                EventID = entry.EventID,
                EventName = entry.Event.EventName,
                ParticipantID = entry.ParticipantID,
                ParticipantName =
                    $"{entry.Participant.Name} {entry.Participant.Surname}",
                CategoryID = entry.CategoryID,
                CategoryName = entry.Category.CategoryName,
                RouteID = entry.RouteID,
                RouteName = entry.Route?.RouteName,
                Price = entry.Price
            };
        }
    }
}
