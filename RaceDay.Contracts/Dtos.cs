using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    // ==================== AUTH DTOs ====================
    public class RegisterRequest
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // ==================== PROFILE DTOs ====================
    public class ProfileDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    // ==================== EVENT DTOs ====================
    public class CreateEventRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, Range(0.1, 500)]
        public double DistanceKm { get; set; }

        [Required, StringLength(50)]
        public string EventType { get; set; } = string.Empty; // Road, Trail, Cycling
    }

    public class UpdateEventRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, Range(0.1, 500)]
        public double DistanceKm { get; set; }

        [Required, StringLength(50)]
        public string EventType { get; set; } = string.Empty;
    }

    public class EventDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
    }

    // ==================== CATEGORY DTOs ====================
    public class CreateCategoryRequest
    {
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required, Range(0, 100)]
        public int MinAge { get; set; }

        [Required, Range(0, 100)]
        public int MaxAge { get; set; }

        [Required, Range(0, 1000)]
        public decimal EntryFee { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public decimal EntryFee { get; set; }
        public int EventId { get; set; }
    }

    // ==================== ENROLMENT DTOs ====================
    public class CreateEnrolmentRequest
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateEnrolmentStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Confirmed, Cancelled
    }

    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string? BibNumber { get; set; }
    }

    // ==================== RESULT DTOs ====================
    public class CreateResultRequest
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required, Range(1, 10000)]
        public int FinishingPosition { get; set; }
    }

    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int FinishingPosition { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    // ==================== GENERIC RESPONSE ====================
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}