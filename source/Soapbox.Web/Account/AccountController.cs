namespace Soapbox.Web.Account;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Soapbox.Application.Email.Abstractions;
using Soapbox.Application.Settings;
using Soapbox.Domain.Users;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Identity;
using Soapbox.Web.Identity.Managers;

[Authorize]
public partial class AccountController : SoapboxControllerBase
{
    private readonly SiteSettings _siteSettings;
    private readonly AccountService _accountService;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;
    private readonly UrlEncoder _urlEncoder;

    public AccountController(IOptionsSnapshot<SiteSettings> siteSettings,
        AccountService accountService,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        IEmailService emailService,
        ILogger<AccountController> logger,
        UrlEncoder urlEncoder)
    {
        _siteSettings = siteSettings.Value;
        _accountService = accountService;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger;
        _urlEncoder = urlEncoder;
    }
}