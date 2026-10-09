using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public int MinAge { get; set; }

        public int MaxAge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EntryFee { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        // Navigation
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}