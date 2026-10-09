using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [ForeignKey(nameof(ParticipantId))]
        public AppUser? Participant { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string? BibNumber { get; set; }

        // Navigation
        public Result? Result { get; set; }
    }
}
