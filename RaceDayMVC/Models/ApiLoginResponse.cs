namespace RaceDayMVC.Models
{
    public class ApiLoginResponse
    {
        public int AccountID { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string AccountType { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
