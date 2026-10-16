using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Results;
using RaceDayAPI.Model;

namespace RaceDayAPI.Service
{
    public class ResultService
    {
        private readonly RaceDayDBContext _context;

        public ResultService(RaceDayDBContext context)
        {
            _context = context;
        }

        // Retrieve results for the authenticated participant.
        public async Task<List<ResultResponseDTO>> GetMyResultsAsync(
            int accountId)
        {
            var participant = await _context.Participants
                .SingleOrDefaultAsync(p => p.AccountID == accountId);

            if (participant == null)
            {
                throw new KeyNotFoundException(
                    "Participant profile not found.");
            }

            var results = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Category)
                .Where(r =>
                    r.Entry.ParticipantID == participant.ParticipantID)
                .ToListAsync();

            return results
                .Select(MapToResponse)
                .ToList();
        }

        // Retrieve results for an event owned by the organizer.
        public async Task<List<ResultResponseDTO>> GetByEventAsync(
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
                    "You cannot view results for another organizer's event.");
            }

            var results = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Entry.EventID == eventId)
                .ToListAsync();

            return results
                .Select(MapToResponse)
                .ToList();
        }

        // Record a result for an entry belonging to the organizer's event.
        public async Task<ResultResponseDTO> RecordAsync(
            int accountId,
            RecordResultDTO dto)
        {
            var entry = await _context.Entries
                .Include(e => e.Event)
                    .ThenInclude(e => e.Organizer)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .SingleOrDefaultAsync(e => e.EntryID == dto.EntryID);

            if (entry == null)
            {
                throw new KeyNotFoundException("Entry not found.");
            }

            if (entry.Event.Organizer.AccountID != accountId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot record results for another organizer's event.");
            }

            if (entry.Event.EventDate.Date > DateTime.Today)
            {
                throw new InvalidOperationException(
                    "Results cannot be recorded before the event takes place.");
            }

            bool resultExists = await _context.Results
                .AnyAsync(r => r.EntryID == dto.EntryID);

            if (resultExists)
            {
                throw new InvalidOperationException(
                    "A result has already been recorded for this entry.");
            }

            ValidateResult(
                dto.TimeCompleted,
                dto.FinishingPosition);

            var result = new Result
            {
                EntryID = entry.EntryID,
                TimeCompleted = dto.TimeCompleted,
                FinishingPosition = dto.FinishingPosition
            };

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            var createdResult = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Category)
                .SingleAsync(r => r.ResultID == result.ResultID);

            return MapToResponse(createdResult);
        }

        // Update an existing result.
        public async Task<ResultResponseDTO?> UpdateAsync(
            int accountId,
            int resultId,
            UpdateResultDTO dto)
        {
            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Event)
                        .ThenInclude(e => e.Organizer)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e.Category)
                .SingleOrDefaultAsync(r => r.ResultID == resultId);

            if (result == null)
            {
                return null;
            }

            if (result.Entry.Event.Organizer.AccountID != accountId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update results for another organizer's event.");
            }

            ValidateResult(
                dto.TimeCompleted,
                dto.FinishingPosition);

            result.TimeCompleted = dto.TimeCompleted;
            result.FinishingPosition = dto.FinishingPosition;

            await _context.SaveChangesAsync();

            return MapToResponse(result);
        }

        // Validate result values.
        private static void ValidateResult(
            TimeSpan timeCompleted,
            int finishingPosition)
        {
            if (timeCompleted <= TimeSpan.Zero ||
                timeCompleted >= TimeSpan.FromDays(1))
            {
                throw new ArgumentException(
                    "Finish time must be greater than zero and less than 24 hours.");
            }

            if (finishingPosition < 1)
            {
                throw new ArgumentException(
                    "Finishing position must be at least 1.");
            }
        }

        // Map a result and its related entry information to the response DTO.
        private static ResultResponseDTO MapToResponse(Result result)
        {
            return new ResultResponseDTO
            {
                ResultID = result.ResultID,
                EntryID = result.EntryID,
                EventID = result.Entry.EventID,
                EventName = result.Entry.Event.EventName,
                ParticipantID = result.Entry.ParticipantID,
                ParticipantName =
                    $"{result.Entry.Participant.Name} " +
                    $"{result.Entry.Participant.Surname}",
                CategoryID = result.Entry.CategoryID,
                CategoryName = result.Entry.Category.CategoryName,
                TimeCompleted = result.TimeCompleted,
                FinishingPosition = result.FinishingPosition
            };
        }
    }
}

