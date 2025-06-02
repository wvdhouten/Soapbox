namespace Soapbox.Identity.Deletion.DeleteAccount;

using System.ComponentModel.DataAnnotations;

public class DeleteAccountRequest
{
    public bool RequirePassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
