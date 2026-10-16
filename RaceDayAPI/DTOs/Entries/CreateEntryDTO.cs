using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Entries
{
    public class CreateEntryDTO
    {
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        public int? RouteID { get; set; }

        [Range(typeof(decimal), "0", "100000")]
        public decimal Price { get; set; }
    }
}
