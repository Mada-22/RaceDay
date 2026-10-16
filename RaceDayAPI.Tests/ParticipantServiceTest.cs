
using Microsoft.EntityFrameworkCore;
using RaceDay.Services;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Profiles;
using RaceDayAPI.Model;
using Xunit;

namespace RaceDay.Tests
{
    public class ParticipantServiceTests
    {
        private RaceDayDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDBContext(options);
        }

        private async Task SeedParticipantAsync(RaceDayDBContext context)
        {
            var account = new Account
            {
                AccountID = 1,
                UserName = "testparticipant",
                PasswordHash = "TEST_HASH",
                UserEmail = "participant@test.com",
                AccountType = "Participant"
            };

            var participant = new Participant
            {
                ParticipantID = 1,
                AccountID = 1,
                Account = account,
                Name = "John",
                Surname = "Smith",
                Age = 25,
                PhoneNumber = "0821234567"
            };

            context.Accounts.Add(account);
            context.Participants.Add(participant);

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetProfileAsync_ProfileExists_ReturnsProfile()
        {
            // Arrange
            using var context = CreateContext();
            await SeedParticipantAsync(context);

            var service = new ParticipantService(context);

            // Act
            var result = await service.GetProfileAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ParticipantID);
            Assert.Equal("John", result.Name);
            Assert.Equal("Smith", result.Surname);
            Assert.Equal(25, result.Age);
            Assert.Equal("testparticipant", result.UserName);
            Assert.Equal("participant@test.com", result.UserEmail);
        }

        [Fact]
        public async Task GetProfileAsync_ProfileDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var service = new ParticipantService(context);

            // Act
            var result = await service.GetProfileAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesProfile()
        {
            // Arrange
            using var context = CreateContext();
            await SeedParticipantAsync(context);

            var service = new ParticipantService(context);

            var dto = new UpdateParticipantProfileDTO
            {
                Name = " Michael ",
                Surname = " Jones ",
                Age = 30,
                PhoneNumber = " 0831234567 "
            };

            // Act
            var result = await service.UpdateProfileAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Michael", result.Name);
            Assert.Equal("Jones", result.Surname);
            Assert.Equal(30, result.Age);
            Assert.Equal("0831234567", result.PhoneNumber);

            var savedParticipant = await context.Participants
                .SingleAsync(p => p.AccountID == 1);

            Assert.Equal("Michael", savedParticipant.Name);
            Assert.Equal("Jones", savedParticipant.Surname);
            Assert.Equal(30, savedParticipant.Age);
            Assert.Equal("0831234567", savedParticipant.PhoneNumber);
        }

        [Fact]
        public async Task UpdateProfileAsync_ProfileDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var service = new ParticipantService(context);

            var dto = new UpdateParticipantProfileDTO
            {
                Name = "Michael",
                Surname = "Jones",
                Age = 30,
                PhoneNumber = "0831234567"
            };

            // Act
            var result = await service.UpdateProfileAsync(999, dto);

            // Assert
            Assert.Null(result);
        }
    }
}


