using System.ComponentModel.DataAnnotations;

namespace YumDash.Web.Models;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateTime ReservationDate { get; set; }

    [Range(1, 24)]
    public int PartySize { get; set; }

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [Range(0, 500)]
    public decimal EstimatedSpendPerGuest { get; set; } = 35m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal EstimatedRevenue => PartySize * EstimatedSpendPerGuest;
}
