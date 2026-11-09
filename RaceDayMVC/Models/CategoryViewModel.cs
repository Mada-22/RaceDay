namespace RaceDayMVC.Models
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int? MinimumAge { get; set; }

        public int? MaximumAge { get; set; }

        public decimal EntryFee { get; set; }
        public decimal? Distance { get; set; }
    }
}