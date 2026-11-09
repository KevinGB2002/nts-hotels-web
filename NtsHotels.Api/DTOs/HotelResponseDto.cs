namespace NtsHotels.Api.DTOs;

public class HotelResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool HasStoredImage { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
