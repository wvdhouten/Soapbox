namespace Soapbox.Web.Areas.Admin.Controllers;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Blog.Categories.CreateCategory;
using Soapbox.Application.Blog.Categories.ListCategories;
using Soapbox.Application.Blog.Posts;
using Soapbox.Application.Blog.Posts.Create;
using Soapbox.Application.Blog.Posts.Delete;
using Soapbox.Application.Blog.Posts.Edit;
using Soapbox.Application.Blog.Posts.Get;
using Soapbox.Application.Blog.Posts.List;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator, UserRole.Editor, UserRole.Author)]
public class PostsController : SoapboxControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromServices] ListPostsHandler handler,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await handler.GetPostsPage(page, pageSize, false);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            _ => SomethingWentWrong()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        [FromServices] ListCategoriesHandler handler)
    {
        var result = await handler.GetAllCategoriesAsync(false);
        if (result.IsFailure)
            return SomethingWentWrong();

        ViewData.Add("Categories", result.Value.ToList());
        return View(new CreatePostRequest());
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] ListCategoriesHandler listCategoriesHandler,
        [FromServices] CreateCategoryHandler createCategoryHandler,
        [FromServices] CreatePostHandler handler,
        [FromForm] CreatePostRequest request)
    {
        if (request.Action == "AddCategory")
            return await ProcessNewCategory(listCategoriesHandler, createCategoryHandler, request);

        if (!ModelState.IsValid)
        {
            await AddCategoriesToViewData(listCategoriesHandler);
            return View(request);
        }

        var result = await handler.CreatePostAsync(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Post created successfully.").RedirectToAction(nameof(Index)),
            _ => SomethingWentWrong()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        [FromServices] GetPostHandler handler,
        [FromServices] ListCategoriesHandler categoriesHandler,
        [FromRoute] string id)
    {
        var result = await handler.GetPostByIdAsync(id);
        switch (result)
        {
            case { IsFailure: true, Error.Code: ErrorCode.NotFound }:
                return NotFound(result.Error.Message);
            case { IsSuccess: true, Value: var post }:
                var categoriesResult = await categoriesHandler.GetAllCategoriesAsync(false);
                if (categoriesResult.IsFailure)
                    return SomethingWentWrong();

                ViewData.Add("Categories", categoriesResult.Value);
                return View(new EditPostRequest { Post = post });
            default:
                return SomethingWentWrong();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        [FromServices] ListCategoriesHandler listCategoriesHandler,
        [FromServices] CreateCategoryHandler createCategoryHandler,
        [FromServices] EditPostHandler handler,
        [FromForm] EditPostRequest request)
    {
        if (request.Action == "AddCategory")
            return await ProcessNewCategory(listCategoriesHandler, createCategoryHandler, request);

        if (!ModelState.IsValid)
        {
            await AddCategoriesToViewData(listCategoriesHandler);
            return View(request);
        }

        var result = await handler.UpdatePost(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Post updated successfully.").RedirectToAction(nameof(Edit), new { id = request.Post.Id }),
            _ => SomethingWentWrong()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Delete(
        [FromServices] GetPostHandler handler,
        [FromRoute] string id)
    {
        var result = await handler.GetPostByIdAsync(id);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            _ => SomethingWentWrong()
        };
    }

    [HttpPost]
    public async Task<IActionResult> Delete(
        [FromServices] DeletePostHandler handler,
        [FromRoute] string id)
    {
        var result = await handler.DeletePostAsync(id);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Post deleted successfully.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.Unknown } => WithErrorMessage("Failed to delete post.").RedirectToAction(nameof(Index)),
            _ => SomethingWentWrong()
        };
    }

    private async Task AddCategoriesToViewData(ListCategoriesHandler listCategoriesHandler)
    {
        var categoriesResult = await listCategoriesHandler.GetAllCategoriesAsync(false);
        if (categoriesResult.IsSuccess)
            ViewData.Add("Categories", categoriesResult.Value.ToList());
    }

    private async Task<IActionResult> ProcessNewCategory(ListCategoriesHandler listCategoriesHandler, CreateCategoryHandler createCategoryHandler, ICanAddCategory request)
    {
        // Silently create the category. If something goes wrong, we continue like nothing happened.
        if (request.NewCategory is not null)
        {
            var createCategoryRequest = new CreateCategoryRequest { Category = new() { Name = request.NewCategory! } };
            await createCategoryHandler.CreateCategoryAsync(createCategoryRequest);
        }

        ModelState.Clear();

        await AddCategoriesToViewData(listCategoriesHandler);
        return View(request);
    }
}
