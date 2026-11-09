using System.ComponentModel.DataAnnotations;

namespace NtsHotels.Api.DTOs;

public class HotelCreateForm : HotelCreateDto
{
    [Required]
    public IFormFile Image { get; set; } = null!;
}

public class HotelUpdateForm : HotelUpdateDto
{
    [Required]
    public IFormFile Image { get; set; } = null!;
}
