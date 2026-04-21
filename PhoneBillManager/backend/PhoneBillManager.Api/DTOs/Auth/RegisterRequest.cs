using System.ComponentModel.DataAnnotations;

namespace PhoneBillManager.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
