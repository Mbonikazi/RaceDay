using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public double DistanceKm { get; set; }

        [Required, StringLength(50)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public int OrganiserId { get; set; }

        [ForeignKey(nameof(OrganiserId))]
        public AppUser? Organiser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
