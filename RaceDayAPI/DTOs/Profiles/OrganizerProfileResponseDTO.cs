namespace RaceDayAPI.DTOs.Profiles
{
    public class OrganizerProfileResponseDTO
    {
        public int OrganizerID { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
