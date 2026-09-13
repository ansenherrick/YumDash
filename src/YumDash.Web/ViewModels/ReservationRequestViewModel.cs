using System.ComponentModel.DataAnnotations;

namespace YumDash.Web.ViewModels;

public class ReservationRequestViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Full Name")]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a reservation date and time.")]
    [Display(Name = "Date & Time")]
    public DateTime? ReservationDate { get; set; }

    [Range(1, 24)]
    [Display(Name = "Party Size")]
    public int PartySize { get; set; } = 2;

    [Range(0, 500)]
    [Display(Name = "Estimated Spend / Guest")]
    public decimal EstimatedSpendPerGuest { get; set; } = 35m;

    [StringLength(500)]
    public string? Notes { get; set; }
}
