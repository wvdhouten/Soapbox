namespace Soapbox.Web.Models.Account;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

public class ProfileModel
{
    public string Username { get; set; }

    [Display(Name = "Display name")]
    public string DisplayName { get; set; }

    public string Email { get; set; }

    [EmailAddress]
    [Display(Name = "New email")]
    public string NewEmail { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public IList<UserLoginInfo> CurrentLogins { get; set; }

    public IList<AuthenticationScheme> OtherLogins { get; set; }

    public bool HasAuthenticator { get; set; }

    public int RecoveryCodesLeft { get; set; }

    public bool Is2faEnabled { get; set; }

    public bool IsMachineRemembered { get; set; }
}
