namespace Soapbox.Web.Controllers;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Blog.Archive.GetArchive;
using Soapbox.Application.Blog.Authors;
using Soapbox.Application.Blog.Categories.GetCategory;
using Soapbox.Application.Blog.Categories.ListCategories;
using Soapbox.Application.Blog.Posts.Get;
using Soapbox.Application.Blog.Posts.List;
using Soapbox.Domain.Results;
using Soapbox.Web.Controllers.Base;

[Route("[controller]")]
public class BlogController : SoapboxControllerBase
{
    private const string PostViewName = "_Post";

    [HttpGet("{page:int=1}")]
    public async Task<IActionResult> Index([FromServices] ListPostsHandler handler, [FromRoute] int page = 1, [FromQuery] int pageSize = 5)
    {
        var result = await handler.GetPostsPage(page, pageSize);
        return result.IsSuccess switch
        {
            false => BadRequest("Unable to process request."),
            _ => View(result.Value),
        };
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> PostBySlug([FromServices] GetPostHandler handler, [FromRoute] string slug)
    {
        var result = await handler.GetPostBySlugAsync(slug);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(PostViewName, result.Value),
        };
    }

    [HttpGet("post/{id}")]
    [ActionName("PostView")]
    public async Task<IActionResult> PostById([FromServices] GetPostHandler handler, [FromRoute] string id)
    {
        var result = await handler.GetPostByIdAsync(id);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(PostViewName, result.Value)
        };
    }

    [HttpGet("archive")]
    [HttpGet("archive/{year:int?}")]
    [HttpGet("archive/{year:int?}/{month:int?}")]
    public async Task<IActionResult> Archive(
        [FromServices] GetArchiveHandler handler,
        [FromRoute] int? year = null,
        [FromRoute] int? month = null)
    {
        var result = await handler.GetPostArchiveAsync(
            year ?? DateTime.UtcNow.Year,
            month ?? (year is null ? DateTime.UtcNow.Month : 0));
        return result.IsSuccess switch
        {
            false => BadRequest("Unable to process request."),
            _ => View(result.Value),
        };
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories([FromServices] ListCategoriesHandler handler)
    {
        var result = await handler.GetAllCategoriesAsync(true);
        return result.IsSuccess switch
        {
            false => BadRequest("Unable to process request."),
            _ => View(result.Value),
        };
    }

    [HttpGet("category/{slug}")]
    public async Task<IActionResult> Category([FromServices] GetCategoryHandler handler, [FromRoute] string slug)
    {
        var result = await handler.GetCategoryBySlugAsync(slug);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(PostViewName, result.Value),
        };
    }

    [HttpGet("author/{id}")]
    public async Task<IActionResult> Author([FromServices] GetAuthorHandler handler, [FromRoute] string id)
    {
        var result = await handler.GetAuthorByIdAsync(id);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(result.Value),
        };
    }
}
