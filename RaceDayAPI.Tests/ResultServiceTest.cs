
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Results;
using RaceDayAPI.Model;
using RaceDayAPI.Service;
using Xunit;

namespace RaceDay.Tests
{
    public class ResultServiceTests
    {
        private RaceDayDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDBContext(options);
        }

        private async Task SeedResultDataAsync(
            RaceDayDBContext context,
            DateTime? eventDate = null,
            bool addExistingResult = false)
        {
            var organizerAccount = new Account
            {
                AccountID = 1,
                UserName = "testorganizer",
                PasswordHash = "TEST_HASH",
                UserEmail = "organizer@test.com",
                AccountType = "Organizer"
            };

            var participantAccount = new Account
            {
                AccountID = 2,
                UserName = "testparticipant",
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
                Name = "John",
                Surname = "Smith",
                Age = 25,
                PhoneNumber = "0821234567"
            };

            var actualEventDate = eventDate ?? DateTime.Today.AddDays(-1);

            var raceEvent = new Event
            {
                EventID = 1,
                EventName = "City Run",
                Description = "A test running event",
                Location = "Cape Town",
                EventDate = actualEventDate,
                OpenDate = actualEventDate.AddDays(-30),
                ClosingDate = actualEventDate.AddDays(-1),
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

            var entry = new Entry
            {
                EntryID = 1,
                EventID = 1,
                ParticipantID = 1,
                CategoryID = 1,
                RouteID = null,
                Price = 100,
                Event = raceEvent,
                Participant = participant,
                Category = category
            };

            context.Accounts.AddRange(
                organizerAccount,
                participantAccount);

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

            context.Entries.Add(entry);

            if (addExistingResult)
            {
                context.Results.Add(new Result
                {
                    ResultID = 1,
                    EntryID = 1,
                    Entry = entry,
                    TimeCompleted = TimeSpan.FromMinutes(45),
                    FinishingPosition = 1
                });
            }

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetMyResultsAsync_ParticipantDoesNotExist_ThrowsKeyNotFoundException()
        {
            using var context = CreateContext();
            var service = new ResultService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.GetMyResultsAsync(999));
        }

        [Fact]
        public async Task RecordAsync_ValidData_CreatesResult()
        {
            using var context = CreateContext();
            await SeedResultDataAsync(context);

            var service = new ResultService(context);

            var dto = new RecordResultDTO
            {
                EntryID = 1,
                TimeCompleted = TimeSpan.FromMinutes(42),
                FinishingPosition = 1
            };

            var result = await service.RecordAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.EntryID);
            Assert.Equal(TimeSpan.FromMinutes(42), result.TimeCompleted);
            Assert.Equal(1, result.FinishingPosition);

            var savedResult = await context.Results
                .SingleOrDefaultAsync(r => r.EntryID == 1);

            Assert.NotNull(savedResult);
        }

        [Fact]
        public async Task RecordAsync_OrganizerDoesNotOwnEvent_ThrowsUnauthorizedAccessException()
        {
            using var context = CreateContext();
            await SeedResultDataAsync(context);

            var service = new ResultService(context);

            var dto = new RecordResultDTO
            {
                EntryID = 1,
                TimeCompleted = TimeSpan.FromMinutes(42),
                FinishingPosition = 1
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.RecordAsync(999, dto));

            Assert.Empty(await context.Results.ToListAsync());
        }

        [Fact]
        public async Task RecordAsync_EventHasNotTakenPlace_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            await SeedResultDataAsync(
                context,
                DateTime.Today.AddDays(7));

            var service = new ResultService(context);

            var dto = new RecordResultDTO
            {
                EntryID = 1,
                TimeCompleted = TimeSpan.FromMinutes(42),
                FinishingPosition = 1
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RecordAsync(1, dto));

            Assert.Empty(await context.Results.ToListAsync());
        }

        [Fact]
        public async Task RecordAsync_ResultAlreadyExists_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            await SeedResultDataAsync(
                context,
                addExistingResult: true);

            var service = new ResultService(context);

            var dto = new RecordResultDTO
            {
                EntryID = 1,
                TimeCompleted = TimeSpan.FromMinutes(42),
                FinishingPosition = 2
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RecordAsync(1, dto));
        }

        [Fact]
        public async Task RecordAsync_InvalidFinishingPosition_ThrowsArgumentException()
        {
            using var context = CreateContext();
            await SeedResultDataAsync(context);

            var service = new ResultService(context);

            var dto = new RecordResultDTO
            {
                EntryID = 1,
                TimeCompleted = TimeSpan.FromMinutes(42),
                FinishingPosition = 0
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.RecordAsync(1, dto));

            Assert.Empty(await context.Results.ToListAsync());
        }

        [Fact]
        public async Task UpdateAsync_ValidData_UpdatesResult()
        {
            using var context = CreateContext();

            await SeedResultDataAsync(
                context,
                addExistingResult: true);

            var service = new ResultService(context);

            var dto = new UpdateResultDTO
            {
                TimeCompleted = TimeSpan.FromMinutes(40),
                FinishingPosition = 2
            };

            var result = await service.UpdateAsync(1, 1, dto);

            Assert.NotNull(result);
            Assert.Equal(TimeSpan.FromMinutes(40), result.TimeCompleted);
            Assert.Equal(2, result.FinishingPosition);

            var savedResult = await context.Results.FindAsync(1);

            Assert.NotNull(savedResult);
            Assert.Equal(TimeSpan.FromMinutes(40), savedResult.TimeCompleted);
            Assert.Equal(2, savedResult.FinishingPosition);
        }
    }
}


