using Microsoft.EntityFrameworkCore;
using NtsHotels.Api.Data;
using NtsHotels.Api.DTOs;
using NtsHotels.Api.Models;

namespace NtsHotels.Api.Services;

public class UserService(ApplicationDbContext db) : IUserService
{
    public async Task<UserResponseDto?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user is null) return null;

        return BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash) ? ToDto(user) : null;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await db.Users.AsNoTracking().Include(u => u.Role)
            .OrderBy(u => u.FullName).ToListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        var user = await db.Users.AsNoTracking().Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(email, null);
        var role = await GetRoleAsync(dto.RoleId);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
            Email = email,
            RoleId = role.Id,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<bool> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;

        var email = dto.Email.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(email, id);
        await GetRoleAsync(dto.RoleId);

        user.FullName = dto.FullName.Trim();
        user.Email = email;
        user.RoleId = dto.RoleId;
        user.IsActive = dto.IsActive;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;

        user.IsActive = false;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task EnsureEmailAvailableAsync(string email, Guid? excludeId)
    {
        if (await db.Users.AnyAsync(u => u.Email == email && u.Id != excludeId))
            throw new InvalidOperationException("El email ya está registrado.");
    }

    private async Task<Role> GetRoleAsync(int roleId)
        => await db.Roles.FindAsync(roleId)
           ?? throw new InvalidOperationException("El rol especificado no existe.");

    private static UserResponseDto ToDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        RoleId = u.RoleId,
        RoleName = u.Role.Name,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
