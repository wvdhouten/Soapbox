namespace Soapbox.Identity.Authentication.Login;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Soapbox.Domain.Results;

public class LoginRequest
{
    public const string RequiresTwoFactor = nameof(RequiresTwoFactor);
    public const string LockedOut = nameof(LockedOut);
    public const string Unconfirmed = nameof(Unconfirmed);

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = [];

    public string? ReturnUrl { get; set; }

    [Required]
    [MinLength(8)]
    [Display(Name = "Username", Prompt = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password", Prompt = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public LoginRequest WithExternalLogins(IEnumerable<AuthenticationScheme> externalLogins)
    {
        ExternalLogins = [.. externalLogins];
        return this;
    }
}
