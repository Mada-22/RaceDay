using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Authentication;
using RaceDayAPI.Model;
using RaceDayAPI.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class AccountServiceTests
    {
        private readonly SqliteConnection _connection;
        private readonly RaceDayDBContext _context;
        private readonly AccountService _service;

        public AccountServiceTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<RaceDayDBContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new RaceDayDBContext(options);
            _context.Database.EnsureCreated();

            _service = new AccountService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private ParticipantRegistrationDTO CreateParticipantDTO()
        {
            return new ParticipantRegistrationDTO
            {
                UserName = "testparticipant",
                UserEmail = "participant@test.com",
                Password = "SecurePassword123!",
                Name = "John",
                Surname = "Smith",
                Age = 25,
                PhoneNumber = "0821234567"
            };
        }

        private OrganizerRegistrationDTO CreateOrganizerDTO()
        {
            return new OrganizerRegistrationDTO
            {
                UserName = "testorganizer",
                UserEmail = "organizer@test.com",
                Password = "SecurePassword123!",
                OrganizationName = "Cape Town Running Club",
                Address = "Cape Town",
                PhoneNumber = "0211234567",
                Email = "club@test.com"
            };
        }

        private async Task<Account> SeedAccountAsync(
            string username = "existinguser",
            string email = "existing@test.com",
            string password = "SecurePassword123!")
        {
            var account = new Account
            {
                UserName = username,
                UserEmail = email,
                AccountType = "Participant"
            };

            var hasher = new PasswordHasher<Account>();
            account.PasswordHash = hasher.HashPassword(account, password);

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return account;
        }

        // 1. Participant registration creates both the account and profile.

        [Fact]
        public async Task RegisterParticipantAsync_ValidData_CreatesAccountAndParticipant()
        {
            var dto = CreateParticipantDTO();

            var account = await _service.RegisterParticipantAsync(dto);

            Assert.NotEqual(0, account.AccountID);
            Assert.Equal("testparticipant", account.UserName);
            Assert.Equal("participant@test.com", account.UserEmail);
            Assert.Equal("Participant", account.AccountType);

            Assert.NotEqual("SecurePassword123!", account.PasswordHash);

            var savedAccount = await _context.Accounts
                .SingleAsync(a => a.AccountID == account.AccountID);

            var participant = await _context.Participants
                .SingleAsync(p => p.AccountID == account.AccountID);

            Assert.Equal(savedAccount.AccountID, participant.AccountID);
            Assert.Equal("John", participant.Name);
            Assert.Equal("Smith", participant.Surname);
            Assert.Equal(25, participant.Age);
        }

        // 2. Organizer registration creates both the account and profile.

        [Fact]
        public async Task RegisterOrganizerAsync_ValidData_CreatesAccountAndOrganizer()
        {
            var dto = CreateOrganizerDTO();

            var account = await _service.RegisterOrganizerAsync(dto);

            Assert.NotEqual(0, account.AccountID);
            Assert.Equal("testorganizer", account.UserName);
            Assert.Equal("organizer@test.com", account.UserEmail);
            Assert.Equal("Organizer", account.AccountType);

            Assert.NotEqual("SecurePassword123!", account.PasswordHash);

            var organizer = await _context.Organizers
                .SingleAsync(o => o.AccountID == account.AccountID);

            Assert.Equal("Cape Town Running Club", organizer.OrganizationName);
            Assert.Equal("Cape Town", organizer.Address);
            Assert.Equal("club@test.com", organizer.Email);
        }

        // 3. Duplicate username is rejected during participant registration.

        [Fact]
        public async Task RegisterParticipantAsync_DuplicateUsername_ThrowsInvalidOperationException()
        {
            await SeedAccountAsync();

            var dto = CreateParticipantDTO();
            dto.UserName = "existinguser";

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegisterParticipantAsync(dto));

            Assert.Single(await _context.Accounts.ToListAsync());
            Assert.Empty(await _context.Participants.ToListAsync());
        }

        // 4. Duplicate email is rejected during organizer registration.

        [Fact]
        public async Task RegisterOrganizerAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            await SeedAccountAsync();

            var dto = CreateOrganizerDTO();
            dto.UserEmail = "existing@test.com";

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegisterOrganizerAsync(dto));

            Assert.Single(await _context.Accounts.ToListAsync());
            Assert.Empty(await _context.Organizers.ToListAsync());
        }

        // 5. Correct username and password allow login.

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAccount()
        {
            var account = await SeedAccountAsync();

            var dto = new LoginDTO
            {
                UserNameOrEmail = "existinguser",
                Password = "SecurePassword123!"
            };

            var result = await _service.LoginAsync(dto);

            Assert.Equal(account.AccountID, result.AccountID);
            Assert.Equal("existinguser", result.UserName);
        }

        // 6. An unknown username is rejected.

        [Fact]
        public async Task LoginAsync_UnknownUsername_ThrowsUnauthorizedAccessException()
        {
            var dto = new LoginDTO
            {
                UserNameOrEmail = "unknownuser",
                Password = "SecurePassword123!"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.LoginAsync(dto));
        }

        // 7. An incorrect password is rejected.

        [Fact]
        public async Task LoginAsync_IncorrectPassword_ThrowsUnauthorizedAccessException()
        {
            await SeedAccountAsync();

            var dto = new LoginDTO
            {
                UserNameOrEmail = "existinguser",
                Password = "WrongPassword123!"
            };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.LoginAsync(dto));
        }
    }
}


