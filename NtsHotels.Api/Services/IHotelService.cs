using NtsHotels.Api.DTOs;

namespace NtsHotels.Api.Services;

public interface IHotelService
{
    Task<IEnumerable<HotelResponseDto>> GetAllAsync(string? name, int? rating);
    Task<HotelResponseDto?> GetByIdAsync(Guid id);
    Task<HotelResponseDto> CreateAsync(HotelCreateDto dto);
    Task<bool> UpdateAsync(Guid id, HotelUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
