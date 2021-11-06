namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Common;
    using Soapbox.Core.Email;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Users;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;
    using Soapbox.Web.Services;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class UsersController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<SoapboxUser> _userManager;
        private readonly IEmailClient _emailClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAccountService accountService, UserManager<SoapboxUser> userManager, IEmailClient emailClient, ILogger<UsersController> logger)
        {
            _accountService = accountService;
            _userManager = userManager;
            _emailClient = emailClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData[Constants.Title] = "Users";

            var users = _userManager.Users.AsEnumerable();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData[Constants.Title] = "New user";

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
                await _emailClient.SendEmailAsync(user.Email, "Reset Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ViewData[Constants.Title] = "Edit user";

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

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _accountService.FindUserByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            // TODO: Delegate to service
            var userId = User.GetUserId<string>();
            if (user.Id == userId)
            {
                throw new InvalidOperationException("You cannot delete yourself.");
            }

            await _accountService.DeleteUserAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
