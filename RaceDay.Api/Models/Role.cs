using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
