using System.ComponentModel.DataAnnotations;

namespace YumDash.Web.Models;

public class EventInquiry
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    public DateTime? EventDate { get; set; }

    [Range(1, 500)]
    public int PartySize { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
