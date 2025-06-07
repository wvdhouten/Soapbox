namespace Soapbox.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Users.CreateUser;
using Soapbox.Identity.Users.DeleteUser;
using Soapbox.Identity.Users.EditUser;
using Soapbox.Identity.Users.GetUser;
using Soapbox.Identity.Users.ListUsers;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;
using System.Threading.Tasks;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator)]
public class UsersController : SoapboxControllerBase
{
    [HttpGet]
    public IActionResult Index([FromServices] ListUsersHandler handler, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = handler.GetUsersPage(page, pageSize);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateUserRequest());

    [HttpPost]
    public async Task<IActionResult> Create([FromServices] CreateUserHandler handler, [FromForm] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.CreateUserAsync(request);
        return result switch
        {
            { IsSuccess: true } => RedirectToAction(nameof(Index)),
            { IsFailure: true } when result.Error is ValidationError error => ValidationError(nameof(Create), request, error.Errors),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Edit([FromServices] GetUserHandler handler, string id)
    {
        var result = await handler.GetUserById(id);
        return result switch
        {
            { IsSuccess: true, Value: SoapboxUser user } => View(new EditUserRequest
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                Role = user.Role
            }),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound("User not found."),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromServices] EditUserHandler handler, [FromForm] EditUserRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.EditUserAsync(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("User updated.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound("User not found."),
            { IsFailure: true } when result.Error is ValidationError error => ValidationError(nameof(Edit), request, error.Errors),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Delete([FromServices] GetUserHandler handler, string id)
    {
        // TODO! Implement a confirmation view for deletion.

        var result = await handler.GetUserById(id);
        return result switch
        {
            { IsSuccess: true, Value: SoapboxUser user } => View(),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound("User not found."),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromServices] DeleteUserHandler handler, [FromBody]string id)
    {
        var result = await handler.DeleteUserAsync(id);
        return result switch
        {
            { IsSuccess: true } => RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            { IsFailure: true, Error.Code: ErrorCode.InvalidOperation } => BadRequest(result.Error.Message),
            _ => BadRequest("Something went wrong.")
        };
    }
}
