namespace Soapbox.Web.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Blog.Categories.CreateCategory;
using Soapbox.Application.Blog.Categories.DeleteCategory;
using Soapbox.Application.Blog.Categories.GetCategory;
using Soapbox.Application.Blog.Categories.ListCategories;
using Soapbox.Application.Blog.Categories.UpdateCategory;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;
using System.Threading.Tasks;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator, UserRole.Editor)]
public class CategoriesController : SoapboxControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromServices] ListCategoriesHandler handler, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 25)
    {
        var result = await handler.GetCategoryPageAsync(page, pageSize);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateCategoryRequest());

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateCategoryHandler handler,
        [FromForm] CreateCategoryRequest request)
    {
        var result = await handler.CreateCategoryAsync(request);
        return result switch
        {
            { IsSuccess: true } => RedirectToAction(nameof(Index)),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        [FromServices] GetCategoryHandler handler,
        [FromRoute] string id)
    {
        var result = await handler.GetCategoryByIdAsync(id);
        return result switch
        {
            { IsSuccess: true, Value: var category } => View(UpdateCategoryRequest.FromCategory(category)),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        [FromServices] UpdateCategoryHandler handler, 
        [FromForm] UpdateCategoryRequest request)
    {
        var result = await handler.UpdateCategoryAsync(request);
        return result switch
        {
            { IsSuccess: true } => RedirectToAction(nameof(Index), new { id = request.Category.Id }),
            _ => BadRequest("Something went wrong.")
        };
    }

    [HttpPost]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteCategoryHandler handler,
        [FromRoute] string id)
    {
        var result = await handler.DeleteCategoryByIdAsync(id);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Category deleted.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.Unknown } => WithErrorMessage(result.Error.Message).RedirectToAction(nameof(Index)),
            _ => BadRequest("Something went wrong.")
        };
    }
}
