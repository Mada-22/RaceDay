
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Model;
using RaceDayAPI.Service;
using RaceDayAPI.DTOs.Profiles;
using Xunit;

namespace RaceDay.Tests
{
    public class OrganizerServiceTests
    {
        private RaceDayDBContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDBContext(options);
        }

        private async Task SeedOrganizerAsync(RaceDayDBContext context)
        {
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
                OrganizationName = "Original Organization",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "contact@test.com"
            };

            context.Accounts.Add(account);
            context.Organizers.Add(organizer);

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetProfileAsync_ProfileExists_ReturnsProfile()
        {
            // Arrange
            using var context = CreateContext();
            await SeedOrganizerAsync(context);

            var service = new OrganizerService(context);

            // Act
            var result = await service.GetProfileAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrganizerID);
            Assert.Equal("Original Organization", result.OrganizationName);
            Assert.Equal("testorganizer", result.UserName);
            Assert.Equal("organizer@test.com", result.UserEmail);
        }

        [Fact]
        public async Task GetProfileAsync_ProfileDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var service = new OrganizerService(context);

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
            await SeedOrganizerAsync(context);

            var service = new OrganizerService(context);

            var dto = new UpdateOrganizerProfileDTO
            {
                OrganizationName = "Updated Organization",
                Address = "Stellenbosch",
                PhoneNumber = "0821234567",
                Email = "updated@test.com"
            };

            // Act
            var result = await service.UpdateProfileAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Organization", result.OrganizationName);
            Assert.Equal("Stellenbosch", result.Address);
            Assert.Equal("0821234567", result.PhoneNumber);
            Assert.Equal("updated@test.com", result.Email);

            var savedOrganizer = await context.Organizers
                .SingleAsync(o => o.AccountID == 1);

            Assert.Equal("Updated Organization", savedOrganizer.OrganizationName);
        }

        [Fact]
        public async Task UpdateProfileAsync_ProfileDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var service = new OrganizerService(context);

            var dto = new UpdateOrganizerProfileDTO
            {
                OrganizationName = "Updated Organization",
                Address = "Stellenbosch",
                PhoneNumber = "0821234567",
                Email = "updated@test.com"
            };

            // Act
            var result = await service.UpdateProfileAsync(999, dto);

            // Assert
            Assert.Null(result);
        }
    }
}


