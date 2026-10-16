using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Results
{
    public class RecordResultDTO
    {
        [Range(1, int.MaxValue)]
        public int EntryID { get; set; }

        public TimeSpan TimeCompleted { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }
    }
}
