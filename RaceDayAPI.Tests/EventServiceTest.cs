
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.DTOs.Events;
using RaceDayAPI.Model;

using RaceDayAPI.Service;
using Xunit;

namespace RaceDay.Tests
{
    public class EventServiceTests
    {
        private RaceDayDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDBContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEvents()
        {
            // Arrange
            using var context = CreateContext();

            var organizer = new Organizer
            {
                OrganizerID = 1,
                AccountID = 1,
                OrganizationName = "Test Organization",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer@test.com"
            };

            context.Organizers.Add(organizer);

            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "City Run",
                Description = "A city running event",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(30),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = 1,
                Organizer = organizer
            });

            await context.SaveChangesAsync();

            var service = new EventService(context);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_EventDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var service = new EventService(context);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ValidEventAndOrganizer_CreatesEvent()
        {
            // Arrange
            using var context = CreateContext();

            var account = new Account
            {
                AccountID = 1,
                UserName = "testorganizer",
                PasswordHash = "TEST_HASH",
                UserEmail = "organizer@test.com",
                AccountType = "Organizer"
            };

            var organizer = new Organizer
            {
                OrganizerID = 1,
                AccountID = 1,
                Account = account,
                OrganizationName = "Test Organization",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer@test.com"
            };

            context.Accounts.Add(account);
            context.Organizers.Add(organizer);

            await context.SaveChangesAsync();

            var service = new EventService(context);

            var dto = new CreateEventDTO
            {
                EventName = "City Run",
                Description = "A city running event",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(30),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(20),
                Distance = 10,
                EventType = "Run"
            };

            // Act
            var result = await service.CreateAsync(1, dto);

            // Assert
            Assert.NotNull(result);

            var savedEvent = await context.Events
                .FirstOrDefaultAsync(e => e.EventName == "City Run");

            Assert.NotNull(savedEvent);
            Assert.Equal(1, savedEvent.OrganizerID);
        }

        [Fact]
        public async Task UpdateAsync_ValidDataAndEventOwner_UpdatesEvent()
        {
            // Arrange
            using var context = CreateContext();

            var organizer = new Organizer
            {
                OrganizerID = 1,
                AccountID = 1,
                OrganizationName = "Test Organization",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer@test.com"
            };

            context.Organizers.Add(organizer);

            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "Old Run",
                Description = "Original description",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(30),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(20),
                Distance = 5,
                EventType = "Run",
                OrganizerID = 1,
                Organizer = organizer
            });

            await context.SaveChangesAsync();

            var service = new EventService(context);

            var dto = new UpdateEventDTO
            {
                EventName = "Updated Run",
                Description = "Updated description",
                Location = "Stellenbosch",
                EventDate = DateTime.UtcNow.AddDays(40),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(30),
                Distance = 10,
                EventType = "Run"
            };

            // Act
            var result = await service.UpdateAsync(1, 1, dto);

            // Assert
            Assert.NotNull(result);

            var updatedEvent = await context.Events.FindAsync(1);

            Assert.NotNull(updatedEvent);
            Assert.Equal("Updated Run", updatedEvent.EventName);
            Assert.Equal("Stellenbosch", updatedEvent.Location);
        }

        [Fact]
        public async Task UpdateAsync_OrganizerDoesNotOwnEvent_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateContext();

            var organizer = new Organizer
            {
                OrganizerID = 2,
                AccountID = 2,
                OrganizationName = "Another Organization",
                Address = "Cape Town",
                PhoneNumber = "0217654321",
                Email = "another@test.com"
            };

            context.Organizers.Add(organizer);

            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "Private Run",
                Description = "Private event",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(30),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(20),
                Distance = 5,
                EventType = "Run",
                OrganizerID = 2,
                Organizer = organizer
            });

            await context.SaveChangesAsync();

            var service = new EventService(context);

            var dto = new UpdateEventDTO
            {
                EventName = "Attempted Update",
                Description = "Unauthorized update",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(40),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(30),
                Distance = 10,
                EventType = "Run"
            };

            // Act and Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.UpdateAsync(1, 1, dto));
        }

        [Fact]
        public async Task DeleteAsync_ValidEventOwner_DeletesEvent()
        {
            // Arrange
            using var context = CreateContext();

            var organizer = new Organizer
            {
                OrganizerID = 1,
                AccountID = 1,
                OrganizationName = "Test Organization",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "organizer@test.com"
            };

            context.Organizers.Add(organizer);

            context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "City Run",
                Description = "A city running event",
                Location = "Cape Town",
                EventDate = DateTime.UtcNow.AddDays(30),
                OpenDate = DateTime.UtcNow,
                ClosingDate = DateTime.UtcNow.AddDays(20),
                Distance = 10,
                EventType = "Run",
                OrganizerID = 1,
                Organizer = organizer
            });

            await context.SaveChangesAsync();

            var service = new EventService(context);

            // Act
            var result = await service.DeleteAsync(1, 1);

            // Assert
            Assert.True(result);
            Assert.Null(await context.Events.FindAsync(1));
        }
    }
}


