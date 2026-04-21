using System.ComponentModel.DataAnnotations;

namespace PhoneBillManager.Api.DTOs.Lines;

public class AssignLineRequest
{
    [Required, MaxLength(150)]
    public string AssignedName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string AssignedContact { get; set; } = string.Empty;
}
