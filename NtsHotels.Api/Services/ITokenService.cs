using NtsHotels.Api.DTOs;

namespace NtsHotels.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(UserResponseDto user);
}
