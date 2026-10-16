
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.DTOs.Entries;
using RaceDayAPI.Model;

using RaceDayAPI.Service;
using Xunit;

namespace RaceDay.Tests
{
    public class EntryServiceTests
    {
        private RaceDayDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDBContext(options);
        }

        private async Task SeedEntryDataAsync(RaceDayDBContext context)
        {
            var organizerAccount = new Account
            {
                AccountID = 1,
                UserName = "organizer",
                PasswordHash = "TEST_HASH",
                UserEmail = "organizer@test.com",
                AccountType = "Organizer"
            };

            var participantAccount = new Account
            {
                AccountID = 2,
                UserName = "participant",
                PasswordHash = "TEST_HASH",
                UserEmail = "participant@test.com",
                AccountType = "Participant"
            };

            var organizer = new Organizer
            {
                OrganizerID = 1,
                AccountID = 1,
                Account = organizerAccount,
                OrganizationName = "Test Organization",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer@test.com"
            };

            var participant = new Participant
            {
                ParticipantID = 1,
                AccountID = 2,
                Account = participantAccount,
                Name = "Test",
                Surname = "Participant",
                Age = 25,
                PhoneNumber = "0821234567"
            };

            var raceEvent = new Event
            {
                EventID = 1,
                EventName = "City Run",
                Description = "A test running event",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(30),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = 1,
                Organizer = organizer
            };

            var category = new Category
            {
                CategoryID = 1,
                CategoryName = "Senior",
                MinimumAge = 18,
                MaximumAge = 40,
                Distance = 10
            };

            context.Accounts.AddRange(organizerAccount, participantAccount);
            context.Organizers.Add(organizer);
            context.Participants.Add(participant);
            context.Events.Add(raceEvent);
            context.Categories.Add(category);

            context.EventCategories.Add(new EventCategory
            {
                EventID = 1,
                CategoryID = 1,
                Event = raceEvent,
                Category = category
            });

            await context.SaveChangesAsync();
        }

       
[Fact]
public async Task GetMyEntriesAsync_ParticipantDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var context = CreateContext();
            var service = new EntryService(context);

            // Act and Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.GetMyEntriesAsync(999));
        }

        [Fact]
        public async Task GetMyEntriesAsync_ParticipantHasNoEntries_ReturnsEmptyList()
        {
            // Arrange
            using var context = CreateContext();
            await SeedEntryDataAsync(context);

            var service = new EntryService(context);

            // Act
            var result = await service.GetMyEntriesAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task CreateAsync_ValidEntry_CreatesEntry()
        {
            // Arrange
            using var context = CreateContext();
            await SeedEntryDataAsync(context);

            var service = new EntryService(context);

            var dto = new CreateEntryDTO
            {
                EventID = 1,
                CategoryID = 1,
                RouteID = null
            };

            // Act
            var result = await service.CreateAsync(2, dto);

            // Assert
            Assert.NotNull(result);

            var savedEntry = await context.Entries
                .FirstOrDefaultAsync(e =>
                    e.EventID == 1 &&
                    e.ParticipantID == 1);

            Assert.NotNull(savedEntry);
            Assert.Equal(1, savedEntry.CategoryID);
        }

        [Fact]
        public async Task CreateAsync_CategoryNotAssociatedWithEvent_ThrowsInvalidOperationException()
        {
            // Arrange
            using var context = CreateContext();
            await SeedEntryDataAsync(context);

            context.Categories.Add(new Category
            {
                CategoryID = 2,
                CategoryName = "Marathon",
                MinimumAge = 18,
                MaximumAge = 60,
                Distance = 21
            });

            await context.SaveChangesAsync();

            var service = new EntryService(context);

            var dto = new CreateEntryDTO
            {
                EventID = 1,
                CategoryID = 2,
                RouteID = null
            };

            // Act and Assert
                   await Assert.ThrowsAsync<ArgumentException>(
() => service.CreateAsync(2, dto));
        }

        [Fact]
        public async Task CreateAsync_ParticipantAlreadyRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            using var context = CreateContext();
            await SeedEntryDataAsync(context);

            context.Entries.Add(new Entry
            {
                EntryID = 1,
                EventID = 1,
                ParticipantID = 1,
                CategoryID = 1,
                RouteID = null,
                Price = 100
            });

            await context.SaveChangesAsync();

            var service = new EntryService(context);

            var dto = new CreateEntryDTO
            {
                EventID = 1,
                CategoryID = 1,
                RouteID = null
            };

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(2, dto));
        }

       
    }
}

