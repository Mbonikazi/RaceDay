using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [ForeignKey(nameof(EnrolmentId))]
        public Enrolment? Enrolment { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishingPosition { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}