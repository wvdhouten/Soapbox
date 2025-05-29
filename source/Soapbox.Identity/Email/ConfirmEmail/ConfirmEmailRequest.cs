namespace Soapbox.Identity.Email.ConfirmEmail;

public record ConfirmEmailRequest
{
    public string UserId { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}
