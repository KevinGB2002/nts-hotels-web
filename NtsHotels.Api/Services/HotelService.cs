using Microsoft.EntityFrameworkCore;
using NtsHotels.Api.Data;
using NtsHotels.Api.DTOs;
using NtsHotels.Api.Models;

namespace NtsHotels.Api.Services;

public class HotelService(ApplicationDbContext db) : IHotelService
{
    public async Task<IEnumerable<HotelResponseDto>> GetAllAsync(string? name, int? rating)
    {
        var query = db.Hotels.AsNoTracking().Where(h => h.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(h => h.Name.Contains(name));

        if (rating.HasValue)
            query = query.Where(h => h.Rating == rating.Value);

        var hotels = await query.OrderBy(h => h.Name).ToListAsync();
        return hotels.Select(ToDto).ToList();
    }

    public async Task<HotelResponseDto?> GetByIdAsync(Guid id)
    {
        var hotel = await db.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        return hotel is null ? null : ToDto(hotel);
    }

    public async Task<HotelResponseDto> CreateAsync(HotelCreateDto dto)
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Address = dto.Address,
            City = dto.City,
            Rating = dto.Rating,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
        return ToDto(hotel);
    }

    public async Task<bool> UpdateAsync(Guid id, HotelUpdateDto dto)
    {
        var hotel = await db.Hotels.FindAsync(id);
        if (hotel is null) return false;

        hotel.Name = dto.Name;
        hotel.Description = dto.Description;
        hotel.Address = dto.Address;
        hotel.City = dto.City;
        hotel.Rating = dto.Rating;
        hotel.ImageUrl = dto.ImageUrl;
        hotel.IsActive = dto.IsActive;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var hotel = await db.Hotels.FindAsync(id);
        if (hotel is null) return false;

        hotel.IsActive = false;
        await db.SaveChangesAsync();
        return true;
    }

    private static HotelResponseDto ToDto(Hotel h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        Description = h.Description,
        Address = h.Address,
        City = h.City,
        Rating = h.Rating,
        ImageUrl = h.ImageUrl,
        IsActive = h.IsActive,
        CreatedAt = h.CreatedAt
    };
}
