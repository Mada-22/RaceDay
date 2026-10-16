using Microsoft.EntityFrameworkCore;

using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Profiles;

namespace RaceDay.Services
{
    public class ParticipantService
    {
        private readonly RaceDayDBContext _context;

        public ParticipantService(RaceDayDBContext context)
        {
            _context = context;
        }

        public async Task<ParticipantProfileResponseDTO?> GetProfileAsync(
            int accountId)
        {
            var participant = await _context.Participants
                .Include(p => p.Account)
                .SingleOrDefaultAsync(p => p.AccountID == accountId);

            if (participant == null)
            {
                return null;
            }

            return new ParticipantProfileResponseDTO
            {
                ParticipantID = participant.ParticipantID,
                Name = participant.Name,
                Surname = participant.Surname,
                Age = participant.Age,
                PhoneNumber = participant.PhoneNumber,
                UserName = participant.Account.UserName,
                UserEmail = participant.Account.UserEmail
            };
        }

        public async Task<ParticipantProfileResponseDTO?> UpdateProfileAsync(
            int accountId,
            UpdateParticipantProfileDTO dto)
        {
            var participant = await _context.Participants
                .Include(p => p.Account)
                .SingleOrDefaultAsync(p => p.AccountID == accountId);

            if (participant == null)
            {
                return null;
            }

            participant.Name = dto.Name.Trim();
            participant.Surname = dto.Surname.Trim();
            participant.Age = dto.Age;
            participant.PhoneNumber = dto.PhoneNumber.Trim();

            await _context.SaveChangesAsync();

            return new ParticipantProfileResponseDTO
            {
                ParticipantID = participant.ParticipantID,
                Name = participant.Name,
                Surname = participant.Surname,
                Age = participant.Age,
                PhoneNumber = participant.PhoneNumber,
                UserName = participant.Account.UserName,
                UserEmail = participant.Account.UserEmail
            };
        }
    }
}