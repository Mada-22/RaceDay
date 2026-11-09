namespace RaceDayMVC.Models
{
    public class CategoryManagementViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
