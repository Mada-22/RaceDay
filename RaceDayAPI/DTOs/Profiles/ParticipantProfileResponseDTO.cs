namespace RaceDayAPI.DTOs.Profiles
{
    public class ParticipantProfileResponseDTO
    {
        public int ParticipantID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
