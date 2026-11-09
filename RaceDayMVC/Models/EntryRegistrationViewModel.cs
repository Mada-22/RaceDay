namespace RaceDayMVC.Models
{
    public class EntryRegistrationViewModel
    {
        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public int? RouteID { get; set; }

        public List<CategoryViewModel> Categories { get; set; }
            = new();
    }
}