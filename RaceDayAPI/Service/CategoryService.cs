using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Categories;
using RaceDayAPI.Model;

namespace RaceDayAPI.Service
{
    public class CategoryService
    {
        private readonly RaceDayDBContext _context;

        public CategoryService(RaceDayDBContext context)
        {
            _context = context;
        }

       
// Retrieve categories belonging to an event.
public async Task<List<CategoryResponseDTO>> GetByEventAsync(
    int eventId)
        {
            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
            {
                throw new KeyNotFoundException("Event not found.");
            }

            var categories = await _context.EventCategories
                .Where(ec => ec.EventID == eventId)
                .Select(ec => new CategoryResponseDTO
                {
                    CategoryID = ec.Category.CategoryID,
                    CategoryName = ec.Category.CategoryName,
                    MinimumAge = ec.Category.MinimumAge,
                    MaximumAge = ec.Category.MaximumAge,
                    Distance = ec.Category.Distance,
                    EventIDs = new List<int> { ec.EventID }
                })
                .ToListAsync();

            return categories;
        }



        // Create a category and associate it with an event.
        public async Task<CategoryResponseDTO> CreateAsync(
            int accountId,
            int eventId,
            CreateCategoryDTO dto)
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
                    "You cannot manage categories for another organizer's event.");
            }

            ValidateCategory(dto.MinimumAge, dto.MaximumAge, dto.Distance);

            var category = new Category
            {
                CategoryName = dto.CategoryName.Trim(),
                MinimumAge = dto.MinimumAge,
                MaximumAge = dto.MaximumAge,
                Distance = dto.Distance
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            var eventCategory = new EventCategory
            {
                EventID = eventId,
                CategoryID = category.CategoryID
            };

            _context.EventCategories.Add(eventCategory);

            await _context.SaveChangesAsync();

            category.EventCategories = new List<EventCategory>
            {
                eventCategory
            };

            return MapToResponse(category);
        }

        // Update a category associated with an event.
        public async Task<CategoryResponseDTO?> UpdateAsync(
            int accountId,
            int eventId,
            int categoryId,
            UpdateCategoryDTO dto)
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
                    "You cannot manage categories for another organizer's event.");
            }

            bool associationExists = await _context.EventCategories
                .AnyAsync(ec =>
                    ec.EventID == eventId &&
                    ec.CategoryID == categoryId);

            if (!associationExists)
            {
                return null;
            }

            ValidateCategory(dto.MinimumAge, dto.MaximumAge, dto.Distance);

            var category = await _context.Categories
                .Include(c => c.EventCategories)
                .SingleOrDefaultAsync(c => c.CategoryID == categoryId);

            if (category == null)
            {
                return null;
            }

            category.CategoryName = dto.CategoryName.Trim();
            category.MinimumAge = dto.MinimumAge;
            category.MaximumAge = dto.MaximumAge;
            category.Distance = dto.Distance;

            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        // Remove a category association from an event.
        public async Task<bool> DeleteAsync(
            int accountId,
            int eventId,
            int categoryId)
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
                    "You cannot manage categories for another organizer's event.");
            }

            var eventCategory = await _context.EventCategories
                .SingleOrDefaultAsync(ec =>
                    ec.EventID == eventId &&
                    ec.CategoryID == categoryId);

            if (eventCategory == null)
            {
                return false;
            }

            bool categoryInUse = await _context.Entries
                .AnyAsync(entry =>
                    entry.EventID == eventId &&
                    entry.CategoryID == categoryId);

            if (categoryInUse)
            {
                throw new InvalidOperationException(
                    "This category cannot be removed because participants have registered under it.");
            }

            _context.EventCategories.Remove(eventCategory);

            await _context.SaveChangesAsync();

            return true;
        }

        // Validate optional age and distance restrictions.
        private static void ValidateCategory(
            int? minimumAge,
            int? maximumAge,
            decimal? distance)
        {
            if (minimumAge.HasValue && minimumAge.Value < 18)
            {
                throw new ArgumentException(
                    "The minimum age cannot be below 18.");
            }

            if (maximumAge.HasValue && maximumAge.Value < 18)
            {
                throw new ArgumentException(
                    "The maximum age cannot be below 18.");
            }

            if (minimumAge.HasValue &&
                maximumAge.HasValue &&
                maximumAge.Value < minimumAge.Value)
            {
                throw new ArgumentException(
                    "The maximum age cannot be lower than the minimum age.");
            }

            if (distance.HasValue && distance.Value <= 0)
            {
                throw new ArgumentException(
                    "Category distance must be greater than zero.");
            }
        }

        // Map a category entity to its response DTO.
        private static CategoryResponseDTO MapToResponse(
            Category category)
        {
            return new CategoryResponseDTO
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                MinimumAge = category.MinimumAge,
                MaximumAge = category.MaximumAge,
                Distance = category.Distance,
                EventIDs = category.EventCategories
                    .Select(ec => ec.EventID)
                    .ToList()
            };
        }
    }
}