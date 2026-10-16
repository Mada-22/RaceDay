using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs.Profiles;

namespace RaceDayAPI.Service
{
    public class OrganizerService
    {
        private readonly RaceDayDBContext _context;

        public OrganizerService(RaceDayDBContext context)
        {
            _context = context;
        }

        public async Task<OrganizerProfileResponseDTO?> GetProfileAsync(
            int accountId)
        {
            var organizer = await _context.Organizers
                .Include(o => o.Account)
                .SingleOrDefaultAsync(o => o.AccountID == accountId);

            if (organizer == null)
            {
                return null;
            }

            return new OrganizerProfileResponseDTO
            {
                OrganizerID = organizer.OrganizerID,
                OrganizationName = organizer.OrganizationName,
                Address = organizer.Address,
                PhoneNumber = organizer.PhoneNumber,
                Email = organizer.Email,
                UserName = organizer.Account.UserName,
                UserEmail = organizer.Account.UserEmail
            };
        }

        public async Task<OrganizerProfileResponseDTO?> UpdateProfileAsync(
            int accountId,
            UpdateOrganizerProfileDTO dto)
        {
            var organizer = await _context.Organizers
                .Include(o => o.Account)
                .SingleOrDefaultAsync(o => o.AccountID == accountId);

            if (organizer == null)
            {
                return null;
            }

            organizer.OrganizationName = dto.OrganizationName.Trim();
            organizer.Address = dto.Address.Trim();
            organizer.PhoneNumber = dto.PhoneNumber.Trim();
            organizer.Email = dto.Email.Trim();

            await _context.SaveChangesAsync();

            return new OrganizerProfileResponseDTO
            {
                OrganizerID = organizer.OrganizerID,
                OrganizationName = organizer.OrganizationName,
                Address = organizer.Address,
                PhoneNumber = organizer.PhoneNumber,
                Email = organizer.Email,
                UserName = organizer.Account.UserName,
                UserEmail = organizer.Account.UserEmail
            };
        }
    }
}
