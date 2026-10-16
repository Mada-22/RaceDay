namespace RaceDayAPI.DTOs.Authentication
{
    public class AuthenticationResponseDTO
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public int AccountID { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string AccountType { get; set; } = string.Empty;
    }
}
