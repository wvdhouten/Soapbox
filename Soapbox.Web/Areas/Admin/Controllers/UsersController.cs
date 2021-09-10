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
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Core.Identity;
    using Soapbox.Web.Areas.Admin.ViewModels.Users;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class UsersController : Controller
    {
        private readonly UserManager<SoapboxUser> _userManager;
        private readonly IEmailClient emailClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<SoapboxUser> userManager, IEmailClient emailClient, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            this.emailClient = emailClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index([FromRoute] int page = 0)
        {
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
            if (ModelState.IsValid)
            {
                var user = new SoapboxUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Role = model.Role
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var returnUrl = string.Empty;
                    var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { area = "Identity", userId = user.Id, code, returnUrl }, protocol: Request.Scheme);

                    await emailClient.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            ViewData[Constants.Title] = "Edit user";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new SoapboxUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Role = model.Role
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var returnUrl = string.Empty;
                    var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { area = "Identity", userId = user.Id, code, returnUrl }, protocol: Request.Scheme);

                    await emailClient.SendEmailAsync(model.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            var userId = User.GetUserId<string>();
            if (user.Id == userId)
            {
                throw new InvalidOperationException("Cannot delete yourself");
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
