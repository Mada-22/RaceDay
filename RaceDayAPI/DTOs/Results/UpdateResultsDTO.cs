using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs.Results
{
    public class UpdateResultDTO
    {
        public TimeSpan TimeCompleted { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }
    }
}
