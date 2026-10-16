
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Model
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        public int EntryID { get; set; }

        [Required]
        [Column(TypeName = "time")]
        public TimeSpan TimeCompleted { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }

        [ForeignKey(nameof(EntryID))]
        public Entry Entry { get; set; } = null!;
    }
}