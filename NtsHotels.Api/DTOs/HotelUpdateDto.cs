using System.ComponentModel.DataAnnotations;

namespace NtsHotels.Api.DTOs;

public class HotelUpdateDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
