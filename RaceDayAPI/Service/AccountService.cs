using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Authentication;
using RaceDayAPI.Model;

namespace RaceDayAPI.Services
{
    public class AccountService
    {
        private readonly RaceDayDBContext _context;
        private readonly PasswordHasher<Account> _passwordHasher;

    public AccountService(RaceDayDBContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Account>();
        }

        public async Task<Account> RegisterParticipantAsync(
            ParticipantRegistrationDTO dto)
        {
            string username = dto.UserName.Trim();
            string email = dto.UserEmail.Trim();

            bool accountExists = await _context.Accounts.AnyAsync(a =>
                a.UserName == username || a.UserEmail == email);

            if (accountExists)
            {
                throw new InvalidOperationException(
                    "The username or email address is already registered.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var account = new Account
                {
                    UserName = username,
                    UserEmail = email,
                    AccountType = "Participant"
                };

                account.PasswordHash =
                    _passwordHasher.HashPassword(account, dto.Password);

                _context.Accounts.Add(account);

                await _context.SaveChangesAsync();

                var participant = new Participant
                {
                    Name = dto.Name.Trim(),
                    Surname = dto.Surname.Trim(),
                    Age = dto.Age,
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    AccountID = account.AccountID
                };

                _context.Participants.Add(participant);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return account;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Account> RegisterOrganizerAsync(
            OrganizerRegistrationDTO dto)
        {
            string username = dto.UserName.Trim();
            string email = dto.UserEmail.Trim();

            bool accountExists = await _context.Accounts.AnyAsync(a =>
                a.UserName == username || a.UserEmail == email);

            if (accountExists)
            {
                throw new InvalidOperationException(
                    "The username or email address is already registered.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var account = new Account
                {
                    UserName = username,
                    UserEmail = email,
                    AccountType = "Organizer"
                };

                account.PasswordHash =
                    _passwordHasher.HashPassword(account, dto.Password);

                _context.Accounts.Add(account);

                await _context.SaveChangesAsync();

                var organizer = new Organizer
                {
                    OrganizationName = dto.OrganizationName.Trim(),
                    Address = dto.Address.Trim(),
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Email = dto.Email.Trim(),
                    AccountID = account.AccountID
                };

                _context.Organizers.Add(organizer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return account;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Account> LoginAsync(LoginDTO dto)
        {
            string username = dto.UserNameOrEmail.Trim();

            var account = await _context.Accounts
                .SingleOrDefaultAsync(a => a.UserName == username);

            if (account == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid username or password.");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                account,
                account.PasswordHash,
                dto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException(
                    "Invalid username or password.");
            }

            return account;
        }
    }


}


