using NtsHotels.Api.DTOs;

namespace NtsHotels.Api.Services;

public interface IUserService
{
    Task<UserResponseDto?> LoginAsync(LoginDto dto);
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(Guid id);

    /// <exception cref="InvalidOperationException">Email duplicado o rol inexistente.</exception>
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);

    /// <exception cref="InvalidOperationException">Email duplicado o rol inexistente.</exception>
    Task<bool> UpdateAsync(Guid id, UserUpdateDto dto);

    Task<bool> DeleteAsync(Guid id);
}
