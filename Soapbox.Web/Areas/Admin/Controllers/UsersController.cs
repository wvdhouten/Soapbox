namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Common;
    using Soapbox.Core.Identity;
    using Soapbox.Web.Areas.Admin.Models.Users;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;
    using Soapbox.Web.Services;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator)]
    public class UsersController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAccountService accountService, ILogger<UsersController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData[Constants.Title] = "Users";

            var users = await _accountService.GetUsersAsync();

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

            var result = await _accountService.CreateUserAsync(user);
            if (result.Succeeded)
            {
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
