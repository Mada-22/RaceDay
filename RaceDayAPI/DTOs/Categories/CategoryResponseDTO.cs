namespace RaceDayAPI.DTOs.Categories
{
    public class CategoryResponseDTO
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public int? MinimumAge { get; set; }

        public int? MaximumAge { get; set; }

        public decimal? Distance { get; set; }

        public List<int> EventIDs { get; set; } = new();
    }
}
