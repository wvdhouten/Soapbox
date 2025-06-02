namespace Soapbox.Web.Areas.Admin.Controllers;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Blog.Categories.ListCategories;
using Soapbox.Application.Blog.Posts.Create;
using Soapbox.Application.Blog.Posts.Delete;
using Soapbox.Application.Blog.Posts.Edit;
using Soapbox.Application.Blog.Posts.Get;
using Soapbox.Application.Blog.Posts.List;
using Soapbox.Application.Utils;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Attributes;
using Soapbox.Web.Controllers.Base;
using Soapbox.Web.Helpers;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator, UserRole.Editor, UserRole.Author, UserRole.Contributor)]
public class PostsController : SoapboxControllerBase
{
    private readonly IBlogRepository _blogService;

    public PostsController(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromServices] ListPostsHandler handler, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var result = await handler.GetPostsPage(page, pageSize, false);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            _ => SomethingWentWrong()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Create([FromServices] ListCategoriesHandler handler)
    {
        var result = await handler.GetAllCategoriesAsync(false);
        if (result.IsFailure)
            return SomethingWentWrong();

        ViewData.Add("Categories", result.Value);
        return View(new CreatePostRequest());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromServices] CreatePostHandler handler, [FromServices] ListCategoriesHandler categoriesHandler, [FromForm] CreatePostRequest request, string action)
    {
        var categoriesResult = await categoriesHandler.GetAllCategoriesAsync(false);
        if (categoriesResult.IsFailure)
            return SomethingWentWrong();

        //if (action == "AddCategory")
        //    return AddCategory(model);

        ViewData.Add("Categories", categoriesResult.Value);

        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.CreatePost(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Post created successfully.").RedirectToAction(nameof(Index)),
            _ => SomethingWentWrong()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Edit([FromServices] GetPostByIdHandler handler, [FromServices] ListCategoriesHandler categoriesHandler, string id)
    {
        var result = await handler.GetPostAsync(id);
        switch (result)
        {
            case { IsFailure: true, Error.Code: ErrorCode.NotFound }:
                return NotFound(result.Error.Message);
            case { IsSuccess: true, Value: var post }:
                var categoriesResult = await categoriesHandler.GetAllCategoriesAsync(false);
                if (categoriesResult.IsSuccess)
                    ViewData.Add("Categories", result.Value);

                return categoriesResult switch
                {
                    { IsSuccess: true } => View(new EditPostRequest { Post = post }),
                    _ => SomethingWentWrong()
                };
            default:
                return SomethingWentWrong();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromServices] EditPostHandler handler, [FromServices] ListCategoriesHandler categoriesHandler, [FromForm] EditPostRequest request, string action)
    {
        var categoriesResult = await categoriesHandler.GetAllCategoriesAsync(false);
        if (categoriesResult.IsFailure)
            return SomethingWentWrong();

        //if (action == "AddCategory")
        //    return AddCategory(model);

        ViewData.Add("Categories", categoriesResult.Value);

        if (!ModelState.IsValid)
            return View(request);

        var result = await handler.UpdatePost(request);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Post updated successfully.").RedirectToAction(nameof(Edit), new { id = request.Post.Id }),
            _ => SomethingWentWrong()
        };
    }

    [HttpGet]
    public async Task<IActionResult> Delete([FromServices] GetPostByIdHandler handler, [FromQuery] string id)
    {
        var result = await handler.GetPostAsync(id);
        return result switch
        {
            { IsSuccess: true } => View(result.Value),
            { IsFailure: true, Error.Code: ErrorCode.NotFound } => NotFound(result.Error.Message),
            _ => SomethingWentWrong()
        };
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromServices] DeletePostHandler handler, string id)
    {
        var result = await handler.DeletePostAsync(id);
        return result switch
        {
            { IsSuccess: true } => WithStatusMessage("Post deleted successfully.").RedirectToAction(nameof(Index)),
            { IsFailure: true, Error.Code: ErrorCode.Unknown } => WithErrorMessage("Failed to delete post.").RedirectToAction(nameof(Index)),
            _ => SomethingWentWrong()
        };
    }
}
