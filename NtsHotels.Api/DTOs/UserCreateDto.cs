using System.ComponentModel.DataAnnotations;

namespace NtsHotels.Api.DTOs;

public class UserCreateDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{6,}$",
        ErrorMessage = "La contraseña debe tener al menos 6 caracteres, una mayúscula, un número y un carácter especial.")]
    public string Password { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }
}
