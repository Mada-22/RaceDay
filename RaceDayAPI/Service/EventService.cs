using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Events;
using RaceDayAPI.Model;

namespace RaceDayAPI.Service
{
    public class EventService
    {
        private readonly RaceDayDBContext _context;

        public EventService(RaceDayDBContext context)
        {
            _context = context;
        }

        // Retrieve all events.
        public async Task<List<EventResponseDTO>> GetAllAsync()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .ToListAsync();

            return events
                .Select(MapToResponse)
                .ToList();
        }

        // Retrieve one event by ID.
        public async Task<EventResponseDTO?> GetByIdAsync(int eventId)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organizer)
                .SingleOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null)
            {
                return null;
            }

            return MapToResponse(eventEntity);
        }

        // Create an event for the authenticated organizer.
        public async Task<EventResponseDTO> CreateAsync(
            int accountId,
            CreateEventDTO dto)
        {
            var organizer = await _context.Organizers
                .SingleOrDefaultAsync(o => o.AccountID == accountId);

            if (organizer == null)
            {
                throw new InvalidOperationException(
                    "The authenticated account does not have an organizer profile.");
            }

            ValidateEvent(
                dto.EventName,
                dto.Description,
                dto.Location,
                dto.EventType,
                dto.Distance);

            ValidateEventDates(
                dto.OpenDate,
                dto.ClosingDate,
                dto.EventDate);

            var eventEntity = new Event
            {
                EventName = dto.EventName.Trim(),
                Description = dto.Description.Trim(),
                Location = dto.Location.Trim(),
                EventDate = dto.EventDate.Date,
                OpenDate = dto.OpenDate.Date,
                ClosingDate = dto.ClosingDate.Date,
                Distance = dto.Distance,
                EventType = dto.EventType.Trim(),
                OrganizerID = organizer.OrganizerID
            };

            _context.Events.Add(eventEntity);

            await _context.SaveChangesAsync();

            eventEntity.Organizer = organizer;

            return MapToResponse(eventEntity);
        }

        // Update an event owned by the authenticated organizer.
        public async Task<EventResponseDTO?> UpdateAsync(
            int accountId,
            int eventId,
            UpdateEventDTO dto)
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
                    "You cannot update an event owned by another organizer.");
            }

            ValidateEvent(
                dto.EventName,
                dto.Description,
                dto.Location,
                dto.EventType,
                dto.Distance);

            ValidateEventDates(
                dto.OpenDate,
                dto.ClosingDate,
                dto.EventDate);

            eventEntity.EventName = dto.EventName.Trim();
            eventEntity.Description = dto.Description.Trim();
            eventEntity.Location = dto.Location.Trim();
            eventEntity.EventDate = dto.EventDate.Date;
            eventEntity.OpenDate = dto.OpenDate.Date;
            eventEntity.ClosingDate = dto.ClosingDate.Date;
            eventEntity.Distance = dto.Distance;
            eventEntity.EventType = dto.EventType.Trim();

            await _context.SaveChangesAsync();

            return MapToResponse(eventEntity);
        }

        // Delete an event owned by the authenticated organizer.
        public async Task<bool> DeleteAsync(
            int accountId,
            int eventId)
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
                    "You cannot delete an event owned by another organizer.");
            }

            bool hasEntries = await _context.Entries
                .AnyAsync(entry => entry.EventID == eventId);

            if (hasEntries)
            {
                throw new InvalidOperationException(
                    "An event with registered participants cannot be deleted.");
            }

            bool hasRoutes = await _context.Routes
                .AnyAsync(route => route.EventID == eventId);

            if (hasRoutes)
            {
                throw new InvalidOperationException(
                    "An event with associated routes cannot be deleted.");
            }

            bool hasCategories = await _context.EventCategories
                .AnyAsync(ec => ec.EventID == eventId);

            if (hasCategories)
            {
                throw new InvalidOperationException(
                    "Remove the event's category associations before deleting it.");
            }

            _context.Events.Remove(eventEntity);

            await _context.SaveChangesAsync();

            return true;
        }

        // Validate basic event information.
        private static void ValidateEvent(
            string eventName,
            string description,
            string location,
            string eventType,
            decimal distance)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException(
                    "Event name is required.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(
                    "Event description is required.");
            }

            if (description.Trim().Length > 500)
            {
                throw new ArgumentException(
                    "Event description cannot exceed 500 characters.");
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException(
                    "Event location is required.");
            }

            if (distance <= 0 || distance > 1000)
            {
                throw new ArgumentException(
                    "Event distance must be greater than zero and cannot exceed 1000.");
            }

            string[] validEventTypes =
            {
                "Run",
                "Walk",
                "Cycle"
            };

            if (!validEventTypes.Contains(
                    eventType,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Event type must be Run, Walk, or Cycle.");
            }
        }

        // Validate the event and registration dates.
        private static void ValidateEventDates(
            DateTime openDate,
            DateTime closingDate,
            DateTime eventDate)
        {
            if (closingDate.Date < openDate.Date)
            {
                throw new ArgumentException(
                    "The closing date cannot be earlier than the opening date.");
            }

            if (eventDate.Date < openDate.Date)
            {
                throw new ArgumentException(
                    "The event date cannot be earlier than the opening date.");
            }

            if (closingDate.Date > eventDate.Date)
            {
                throw new ArgumentException(
                    "Registration cannot close after the event date.");
            }
        }

        // Map an event entity to its response DTO.
        private static EventResponseDTO MapToResponse(Event eventEntity)
        {
            return new EventResponseDTO
            {
                EventID = eventEntity.EventID,
                EventName = eventEntity.EventName,
                Description = eventEntity.Description,
                EventDate = eventEntity.EventDate,
                Location = eventEntity.Location,
                Distance = eventEntity.Distance,
                EventType = eventEntity.EventType,
                OpenDate = eventEntity.OpenDate,
                ClosingDate = eventEntity.ClosingDate,
                OrganizerID = eventEntity.OrganizerID,
                OrganizationName = eventEntity.Organizer.OrganizationName
            };
        }
    }
}

