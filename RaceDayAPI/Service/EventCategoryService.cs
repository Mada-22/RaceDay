using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Model;


namespace RaceDayAPI.Services
{
    public class EventCategoryService
    {
        private readonly RaceDayDBContext _context;


    public EventCategoryService(RaceDayDBContext context)
        {
            _context = context;
        }

        // Get all categories assigned to an event.
        public async Task<List<Category>> GetByEventAsync(
            int accountId,
            int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.Organizer.AccountID == accountId);

            if (!eventExists)
            {
                throw new KeyNotFoundException(
                    "Event not found or you do not own this event.");
            }

            return await _context.EventCategories
                .Where(ec => ec.EventID == eventId)
                .Select(ec => ec.Category)
                .ToListAsync();
        }

        // Assign an existing category to an event.
        public async Task AssignCategoryAsync(
            int accountId,
            int eventId,
            int categoryId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.Organizer.AccountID == accountId);

            if (!eventExists)
            {
                throw new KeyNotFoundException(
                    "Event not found or you do not own this event.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == categoryId);

            if (!categoryExists)
            {
                throw new KeyNotFoundException(
                    "Category not found.");
            }

            var alreadyAssigned = await _context.EventCategories
                .AnyAsync(ec =>
                    ec.EventID == eventId &&
                    ec.CategoryID == categoryId);

            if (alreadyAssigned)
            {
                throw new InvalidOperationException(
                    "This category is already assigned to the event.");
            }

            var eventCategory = new EventCategory
            {
                EventID = eventId,
                CategoryID = categoryId
            };

            _context.EventCategories.Add(eventCategory);

            await _context.SaveChangesAsync();
        }

        // Remove a category assignment from an event.
        public async Task RemoveCategoryAsync(
            int accountId,
            int eventId,
            int categoryId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.Organizer.AccountID == accountId);

            if (!eventExists)
            {
                throw new KeyNotFoundException(
                    "Event not found or you do not own this event.");
            }

            var eventCategory = await _context.EventCategories
                .SingleOrDefaultAsync(ec =>
                    ec.EventID == eventId &&
                    ec.CategoryID == categoryId);

            if (eventCategory == null)
            {
                throw new KeyNotFoundException(
                    "This category is not assigned to the event.");
            }

            var categoryInUse = await _context.Entries
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.CategoryID == categoryId);

            if (categoryInUse)
            {
                throw new InvalidOperationException(
                    "This category cannot be removed because participants have entered using it.");
            }

            _context.EventCategories.Remove(eventCategory);

            await _context.SaveChangesAsync();
        }
    }


}

