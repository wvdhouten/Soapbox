namespace Soapbox.Web.Blog;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Application.Extensions;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Web.Blog.Archive;
using Soapbox.Application;
using Soapbox.Domain.Markdown;

[Route("[controller]")]
public class BlogController : Controller
{
    private const string PostView = "_Post";

    private readonly IBlogRepository _blogService;
    private readonly IMarkdownParser _markdownParser;

    public BlogController(IBlogRepository blogService, IMarkdownParser markdownParser)
    {
        _blogService = blogService;
        _markdownParser = markdownParser;
    }

    [HttpGet("{page:int=1}")]
    public async Task<IActionResult> Index([FromRoute] int page = 1, [FromQuery] int pageSize = 5)
    {
        var posts = await _blogService.GetPostsPageAsync(page, pageSize);

        return View(posts);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Post(string slug)
    {
        var post = await _blogService.GetPostBySlugAsync(slug);
        if (post is null)
            return NotFound();

        SetSeoForPost(post);

        return View(PostView, post);
    }

    [HttpGet("post/{id}")]
    [ActionName(nameof(Post))]
    public async Task<IActionResult> PostById(string id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post is null)
            return NotFound();

        SetSeoForPost(post);

        return View(PostView, post);
    }

    [HttpGet("archive")]
    [HttpGet("archive/{year:int?}/{month:int?}")]
    public async Task<IActionResult> Archive(
        [FromServices] GetMonthArchiveQuery query,
        [FromRoute] int year = 0,
        [FromRoute] int month = 0)
    {
        var model = await query.ExecuteAsync(
            year > 0 ? year : DateTime.UtcNow.Year,
            month > 0 ? month : DateTime.UtcNow.Month);

        return View(model);
    }

    [HttpGet("archive/{year:int?}")]
    public async Task<IActionResult> Archive(
        [FromServices] GetYearArchiveQuery query,
        [FromRoute] int year = 0)
    {
        var model = await query.ExecuteAsync(year > 0 ? year : DateTime.UtcNow.Year);

        return View(model);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _blogService.GetAllCategoriesAsync(true);

        return View(categories);
    }

    [HttpGet("category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var category = await _blogService.GetCategoryBySlugAsync(slug, true);
        if (category is null)
            return NotFound();

        return View(category);
    }

    [HttpGet("author/{id}")]
    public async Task<IActionResult> Author(string id)
    {
        var author = await _blogService.GetAuthorByIdAsync(id);
        if (author is null)
            return NotFound();

        return View(author);
    }

    private void SetSeoForPost(Post post)
    {
        ViewData[Constants.PageTitle] = post.Title;
        var content = _markdownParser.ToHtml(post.Content, out var image);
        var description = !string.IsNullOrWhiteSpace(post.Excerpt)
            ? post.Excerpt
            : content;

        ViewData[Constants.Description] = description.StripHtml().Clip(87);
        ViewData[Constants.Keywords] = string.Join(", ", post.Categories.Select(c => c.Name));
        ViewData[Constants.Author] = post.Author.ShownName;
        ViewData[Constants.Image] = image;
        ViewData[Constants.Video] = null;
    }
}
