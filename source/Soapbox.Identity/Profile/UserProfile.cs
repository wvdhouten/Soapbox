namespace Soapbox.Identity.Profile;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

public class UserProfile
{
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Display name")]
    public string? DisplayName { get; set; }

    public string Email { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "New email")]
    public string? NewEmail { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public IList<UserLoginInfo> CurrentLogins { get; set; } = [];

    public IList<AuthenticationScheme> OtherLogins { get; set; } = [];

    public bool HasAuthenticator { get; set; }

    public int RecoveryCodesLeft { get; set; }

    public bool Is2faEnabled { get; set; }

    public bool IsMachineRemembered { get; set; }
}
