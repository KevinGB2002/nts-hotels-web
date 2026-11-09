using Microsoft.EntityFrameworkCore;
using NtsHotels.Api.Data;
using NtsHotels.Api.DTOs;
using NtsHotels.Api.Models;

namespace NtsHotels.Api.Services;

public class HotelService(ApplicationDbContext db, HotelImageProcessor images) : IHotelService
{
    public async Task<IEnumerable<HotelResponseDto>> GetAllAsync(string? name, int? rating)
    {
        var query = db.Hotels.AsNoTracking().Where(h => h.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(h => h.Name.Contains(name));

        if (rating.HasValue)
            query = query.Where(h => h.Rating == rating.Value);

        return await query.OrderBy(h => h.Name).Select(h => new HotelResponseDto
        {
            Id = h.Id, Name = h.Name, Description = h.Description, Address = h.Address,
            City = h.City, Rating = h.Rating,
            HasStoredImage = h.ImageData != null, IsActive = h.IsActive, CreatedAt = h.CreatedAt
        }).ToListAsync();
    }

    public async Task<HotelResponseDto?> GetByIdAsync(Guid id)
    {
        var hotel = await db.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        return hotel is null ? null : ToDto(hotel);
    }

    public Task<byte[]?> GetImageAsync(Guid id) =>
        db.Hotels.AsNoTracking().Where(h => h.Id == id && h.IsActive)
            .Select(h => h.ImageData).FirstOrDefaultAsync();

    public async Task<HotelResponseDto> CreateAsync(HotelCreateDto dto, IFormFile? image = null)
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Address = dto.Address,
            City = dto.City,
            Rating = dto.Rating,
            ImageData = image is null ? null : await images.CompressAsync(image),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
        return ToDto(hotel);
    }

    public async Task<bool> UpdateAsync(Guid id, HotelUpdateDto dto, IFormFile? image = null)
    {
        var hotel = await db.Hotels.FindAsync(id);
        if (hotel is null) return false;

        hotel.Name = dto.Name;
        hotel.Description = dto.Description;
        hotel.Address = dto.Address;
        hotel.City = dto.City;
        hotel.Rating = dto.Rating;
        if (image is not null)
            hotel.ImageData = await images.CompressAsync(image);
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
        HasStoredImage = h.ImageData != null,
        IsActive = h.IsActive,
        CreatedAt = h.CreatedAt
    };
}
