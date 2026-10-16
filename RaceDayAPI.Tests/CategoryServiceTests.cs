
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Service;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task GetByEventAsync_EventDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var service = new CategoryService(context);

            int eventId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.GetByEventAsync(eventId));
        }


        [Fact]
        public async Task GetByEventAsync_EventExistsButHasNoCategories_ReturnsEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            context.Events.Add(new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run"
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.GetByEventAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        
[Fact]
public async Task GetByEventAsync_EventHasCategory_ReturnsCategory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run"
            };

            var category = new RaceDayAPI.Model.Category
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            context.Events.Add(eventEntity);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            context.EventCategories.Add(new RaceDayAPI.Model.EventCategory
            {
                EventID = eventEntity.EventID,
                CategoryID = category.CategoryID
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.GetByEventAsync(eventEntity.EventID);

            // Assert
            var returnedCategory = Assert.Single(result);

            Assert.Equal(category.CategoryID, returnedCategory.CategoryID);
            Assert.Equal("Senior", returnedCategory.CategoryName);
            Assert.Equal(18, returnedCategory.MinimumAge);
            Assert.Equal(39, returnedCategory.MaximumAge);
            Assert.Equal(10m, returnedCategory.Distance);
            Assert.Contains(eventEntity.EventID, returnedCategory.EventIDs);
        }
        
[Fact]
public async Task GetByEventAsync_MultipleEvents_ReturnsOnlyRequestedEventsCategories()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var firstEvent = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run"
            };

            var secondEvent = new RaceDayAPI.Model.Event
            {
                EventName = "Peninsula Charity Walk",
                Description = "A charity walk",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 6, 1),
                OpenDate = new DateTime(2027, 2, 1),
                ClosingDate = new DateTime(2027, 5, 20),
                Distance = 5,
                EventType = "Walk"
            };

            var firstCategory = new RaceDayAPI.Model.Category
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            var secondCategory = new RaceDayAPI.Model.Category
            {
                CategoryName = "Masters",
                MinimumAge = 40,
                MaximumAge = 120,
                Distance = 5
            };

            context.Events.AddRange(firstEvent, secondEvent);
            context.Categories.AddRange(firstCategory, secondCategory);

            await context.SaveChangesAsync();

            context.EventCategories.AddRange(
                new RaceDayAPI.Model.EventCategory
                {
                    EventID = firstEvent.EventID,
                    CategoryID = firstCategory.CategoryID
                },
                new RaceDayAPI.Model.EventCategory
                {
                    EventID = secondEvent.EventID,
                    CategoryID = secondCategory.CategoryID
                });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.GetByEventAsync(firstEvent.EventID);

            // Assert
            var returnedCategory = Assert.Single(result);

            Assert.Equal(firstCategory.CategoryID, returnedCategory.CategoryID);
            Assert.Equal("Senior", returnedCategory.CategoryName);
            Assert.DoesNotContain(
                result,
                category => category.CategoryID == secondCategory.CategoryID);
        }
       
[Fact]
public async Task CreateAsync_ValidCategoryAndEventOwner_CreatesCategory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var account = new RaceDayAPI.Model.Account
            {
                UserName = "organizer1",
                UserEmail = "organizer1@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var organizer = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Cape Town Athletics",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer1@example.com",
                AccountID = account.AccountID
            };

            context.Organizers.Add(organizer);
            await context.SaveChangesAsync();

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = organizer.OrganizerID
            };

            context.Events.Add(eventEntity);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new RaceDayAPI.DTOs.Categories.CreateCategoryDTO
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            // Act
            var result = await service.CreateAsync(
                account.AccountID,
                eventEntity.EventID,
                dto);

            // Assert
            Assert.True(result.CategoryID > 0);
            Assert.Equal("Senior", result.CategoryName);
            Assert.Equal(18, result.MinimumAge);
            Assert.Equal(39, result.MaximumAge);
            Assert.Equal(10m, result.Distance);

            var savedCategory = await context.Categories
                .FindAsync(result.CategoryID);

            Assert.NotNull(savedCategory);

            var associationExists = await context.EventCategories
                .AnyAsync(ec =>
                    ec.EventID == eventEntity.EventID &&
                    ec.CategoryID == result.CategoryID);

            Assert.True(associationExists);
        }

[Fact]
public async Task CreateAsync_OrganizerDoesNotOwnEvent_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var account1 = new RaceDayAPI.Model.Account
            {
                UserName = "organizer1",
                UserEmail = "organizer1@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            var account2 = new RaceDayAPI.Model.Account
            {
                UserName = "organizer2",
                UserEmail = "organizer2@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            context.Accounts.AddRange(account1, account2);
            await context.SaveChangesAsync();

            var organizer1 = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Athletics Club One",
                Address = "Cape Town",
                PhoneNumber = "0211111111",
                Email = "organizer1@example.com",
                AccountID = account1.AccountID
            };

            var organizer2 = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Athletics Club Two",
                Address = "Cape Town",
                PhoneNumber = "0212222222",
                Email = "organizer2@example.com",
                AccountID = account2.AccountID
            };

            context.Organizers.AddRange(organizer1, organizer2);
            await context.SaveChangesAsync();

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = organizer2.OrganizerID
            };

            context.Events.Add(eventEntity);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new RaceDayAPI.DTOs.Categories.CreateCategoryDTO
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.CreateAsync(
                    account1.AccountID,
                    eventEntity.EventID,
                    dto));

            // Verify that no category was created.
            Assert.Empty(await context.Categories.ToListAsync());
        }

[Fact]
public async Task CreateAsync_MaximumAgeBelowMinimumAge_ThrowsArgumentException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var account = new RaceDayAPI.Model.Account
            {
                UserName = "organizer1",
                UserEmail = "organizer1@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var organizer = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Cape Town Athletics",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer1@example.com",
                AccountID = account.AccountID
            };

            context.Organizers.Add(organizer);
            await context.SaveChangesAsync();

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = organizer.OrganizerID
            };

            context.Events.Add(eventEntity);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new RaceDayAPI.DTOs.Categories.CreateCategoryDTO
            {
                CategoryName = "Invalid Age Category",
                MinimumAge = 40,
                MaximumAge = 18,
                Distance = 10
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    account.AccountID,
                    eventEntity.EventID,
                    dto));

            // Verify that no category was saved.
            Assert.Empty(await context.Categories.ToListAsync());
        }
        
[Fact]
public async Task UpdateAsync_ValidDataAndEventOwner_UpdatesCategory()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var account = new RaceDayAPI.Model.Account
            {
                UserName = "organizer1",
                UserEmail = "organizer1@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var organizer = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Cape Town Athletics",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer1@example.com",
                AccountID = account.AccountID
            };

            context.Organizers.Add(organizer);
            await context.SaveChangesAsync();

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = organizer.OrganizerID
            };

            var category = new RaceDayAPI.Model.Category
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            context.Events.Add(eventEntity);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            context.EventCategories.Add(new RaceDayAPI.Model.EventCategory
            {
                EventID = eventEntity.EventID,
                CategoryID = category.CategoryID
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var dto = new RaceDayAPI.DTOs.Categories.UpdateCategoryDTO
            {
                CategoryName = "Masters",
                MinimumAge = 40,
                MaximumAge = 120,
                Distance = 10
            };

            // Act
            var result = await service.UpdateAsync(
                account.AccountID,
                eventEntity.EventID,
                category.CategoryID,
                dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Masters", result.CategoryName);
            Assert.Equal(40, result.MinimumAge);
            Assert.Equal(120, result.MaximumAge);
            Assert.Equal(10m, result.Distance);

            var updatedCategory = await context.Categories
                .FindAsync(category.CategoryID);

            Assert.NotNull(updatedCategory);
            Assert.Equal("Masters", updatedCategory.CategoryName);
        }

        
[Fact]
public async Task UpdateAsync_CategoryNotAssociatedWithEvent_ReturnsNull()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var account = new RaceDayAPI.Model.Account
            {
                UserName = "organizer1",
                UserEmail = "organizer1@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var organizer = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Cape Town Athletics",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer1@example.com",
                AccountID = account.AccountID
            };

            context.Organizers.Add(organizer);
            await context.SaveChangesAsync();

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = organizer.OrganizerID
            };

            var category = new RaceDayAPI.Model.Category
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            context.Events.Add(eventEntity);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // Deliberately do not associate the category with the event.

            var service = new CategoryService(context);

            var dto = new RaceDayAPI.DTOs.Categories.UpdateCategoryDTO
            {
                CategoryName = "Updated Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            // Act
            var result = await service.UpdateAsync(
                account.AccountID,
                eventEntity.EventID,
                category.CategoryID,
                dto);

            // Assert
            Assert.Null(result);

            // Confirm that the category was not changed.
            var unchangedCategory = await context.Categories
                .FindAsync(category.CategoryID);

            Assert.NotNull(unchangedCategory);
            Assert.Equal("Senior", unchangedCategory.CategoryName);
        }
        
[Fact]
public async Task DeleteAsync_ValidEventOwnerAndCategory_RemovesAssociation()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new RaceDayDBContext(options);

            var account = new RaceDayAPI.Model.Account
            {
                UserName = "organizer1",
                UserEmail = "organizer1@example.com",
                PasswordHash = "test-hash",
                AccountType = "Organizer"
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var organizer = new RaceDayAPI.Model.Organizer
            {
                OrganizationName = "Cape Town Athletics",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer1@example.com",
                AccountID = account.AccountID
            };

            context.Organizers.Add(organizer);
            await context.SaveChangesAsync();

            var eventEntity = new RaceDayAPI.Model.Event
            {
                EventName = "Cape Town 10K",
                Description = "A 10 kilometre race",
                Location = "Cape Town",
                EventDate = new DateTime(2027, 5, 1),
                OpenDate = new DateTime(2027, 1, 1),
                ClosingDate = new DateTime(2027, 4, 20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = organizer.OrganizerID
            };

            var category = new RaceDayAPI.Model.Category
            {
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 39,
                Distance = 10
            };

            context.Events.Add(eventEntity);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var eventCategory = new RaceDayAPI.Model.EventCategory
            {
                EventID = eventEntity.EventID,
                CategoryID = category.CategoryID
            };

            context.EventCategories.Add(eventCategory);
            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.DeleteAsync(
                account.AccountID,
                eventEntity.EventID,
                category.CategoryID);

            // Assert
            Assert.True(result);

            var associationExists = await context.EventCategories
                .AnyAsync(ec =>
                    ec.EventID == eventEntity.EventID &&
                    ec.CategoryID == category.CategoryID);

            Assert.False(associationExists);

            var categoryStillExists = await context.Categories
                .AnyAsync(c => c.CategoryID == category.CategoryID);

            Assert.True(categoryStillExists);
        }







    }
}


