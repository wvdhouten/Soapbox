namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Soapbox.Application.Email.Abstractions;
    using Soapbox.Domain.Users;
    using Soapbox.Web.Areas.Admin.Models.Users;
    using Soapbox.Web.Controllers.Base;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;
    using Soapbox.Web.Models.Email;
    using Soapbox.Web.Identity.Managers;
    using Soapbox.Web.Identity;
    using Soapbox.Application.Utils;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class UsersController : SoapboxControllerBase
    {
        private readonly AccountService _accountService;
        private readonly TransactionalUserManager<SoapboxUser> _userManager;
        private readonly IEmailService _emailClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AccountService accountService, TransactionalUserManager<SoapboxUser> userManager, IEmailService emailClient, ILogger<UsersController> logger)
        {
            _accountService = accountService;
            _userManager = userManager;
            _emailClient = emailClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(int page = 1, int pageSize = 25)
        {
            var model = _userManager.Users.OrderBy(u => u.UserName).GetPaged(page, pageSize);

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new SoapboxUser
            {
                UserName = model.Username,
                DisplayName = model.DisplayName,
                Email = model.Email,
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("New user created without password.");

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action("ResetPassword", "Account", new { area = "", code });
                await _emailClient.SendEmailAsync(user.Email, "Reset Password", new ResetPassword { CallbackUrl = callbackUrl });

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _accountService.FindUserByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new SoapboxUser
                {
                    Id = model.Id,
                    UserName = model.Username,
                    Email = model.Email,
                    DisplayName = model.DisplayName,
                    Role = model.Role
                };

                var result = await _accountService.UpdateUserAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            // TODO: More checks

            var user = await _accountService.FindUserByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            var userId = User.GetUserId();
            if (user.Id == userId)
            {
                throw new InvalidOperationException("You cannot delete yourself.");
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
