namespace Soapbox.Web.Models.Account;

using System.ComponentModel.DataAnnotations;

public class DeleteModel
{
    public bool RequirePassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
