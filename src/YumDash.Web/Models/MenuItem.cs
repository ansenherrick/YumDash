using System.ComponentModel.DataAnnotations;

namespace YumDash.Web.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public MenuCategory Category { get; set; }

    [Range(0, 999)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(250)]
    public string Allergens { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}
