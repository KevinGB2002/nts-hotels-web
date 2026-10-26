using System.ComponentModel.DataAnnotations;

namespace NtsHotels.Api.DTOs;

public class UserUpdateDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}
