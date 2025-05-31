namespace Soapbox.Web.Controllers;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Blog.Archive.GetPostArchive;
using Soapbox.Application.Blog.Authors;
using Soapbox.Application.Blog.Categories.GetCategory;
using Soapbox.Application.Blog.Categories.ListCategories;
using Soapbox.Application.Blog.Posts;
using Soapbox.Domain.Results;

[Route("[controller]")]
public class BlogController : Controller
{
    private const string PostViewName = "_Post";

    [HttpGet("{page:int=1}")]
    public async Task<IActionResult> Index([FromServices] GetPostsPageHandler handler, [FromRoute] int page = 1, [FromQuery] int pageSize = 5)
    {
        var result = await handler.GetPostsAsync(page, pageSize);
        return result.IsSuccess switch
        {
            false => BadRequest("Unable to process request."),
            _ => View(result.Value),
        };
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> PostBySlug([FromServices] GetPostBySlugHandler handler, [FromRoute] string slug)
    {
        var result = await handler.GetPostAsync(slug);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(PostViewName, result.Value),
        };
    }

    [HttpGet("post/{id}")]
    [ActionName("PostView")]
    public async Task<IActionResult> PostById([FromServices] GetPostByIdHandler handler, [FromRoute] string id)
    {
        var result = await handler.GetPostAsync(id);
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
        [FromServices] GetPostArchiveHandler handler,
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
        var result = await handler.GetAllCategoriesAsync();
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
