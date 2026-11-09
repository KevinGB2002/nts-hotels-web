using NtsHotels.Api.DTOs;

namespace NtsHotels.Api.Services;

public interface IHotelService
{
    Task<IEnumerable<HotelResponseDto>> GetAllAsync(string? name, int? rating);
    Task<HotelResponseDto?> GetByIdAsync(Guid id);
    Task<byte[]?> GetImageAsync(Guid id);
    Task<HotelResponseDto> CreateAsync(HotelCreateDto dto, IFormFile? image = null);
    Task<bool> UpdateAsync(Guid id, HotelUpdateDto dto, IFormFile? image = null);
    Task<bool> DeleteAsync(Guid id);
}
