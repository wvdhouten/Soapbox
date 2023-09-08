namespace Soapbox.Web.Controllers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Soapbox.Core.Common;
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Core.Extensions;
    using Soapbox.Core.Markdown;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Models.Blog;

    [Route("[controller]")]
    public class BlogController : Controller
    {
        private const string PostView = "_Post";

        private readonly IBlogService _blogService;
        private readonly IMarkdownParser _markdownParser;
        private readonly IEmailRenderer _renderer;

        public BlogController(IBlogService blogService, IMarkdownParser markdownParser, IEmailRenderer renderer)
        {
            _blogService = blogService;
            _markdownParser = markdownParser;
            _renderer = renderer;
        }

        [HttpGet("{page:int=1}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var posts = await _blogService.GetPostsPageAsync(page, 5);

            return View(posts);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Post(string slug)
        {
            var post = await _blogService.GetPostBySlugAsync(slug);
            if (post is null)
            {
                return NotFound();
            }

            UpdateSeoForPost(post);

            return View(PostView, post);
        }

        [HttpGet("post/{id}")]
        [ActionName(nameof(Post))]
        public async Task<IActionResult> PostById(string id)
        {
            var post = await _blogService.GetPostByIdAsync(id);
            if (post is null)
            {
                return NotFound();
            }

            UpdateSeoForPost(post);

            return View(PostView, post);
        }

        [HttpGet("archive")]
        [HttpGet("archive/{year:int?}/{month:int?}")]
        public async Task<IActionResult> Archive(int year = 0, int month = 0)
        {
            year = year > 0 ? year : DateTime.UtcNow.Year;
            month = month > 0 ? month : DateTime.UtcNow.Month;

            var model = await GetMonthModel(year, month);

            return View(model);
        }

        [HttpGet("archive/{year:int?}")]
        public async Task<IActionResult> Archive(int year = 0)
        {
            year = year > 0 ? year : DateTime.UtcNow.Year;

            var model = await GetYearModel(year);

            return View("Archive", model);
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
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet("author/{id}")]
        public async Task<IActionResult> Author(string id)
        {
            var author = await _blogService.GetAuthorByIdAsync(id);
            if (author is null)
            {
                return NotFound();
            }

            return View(author);
        }

        private async Task<ArchiveModel> GetMonthModel(int year, int month)
        {
            var model = new ArchiveModel { Year = year, Month = month };

            var startOfMonth = new DateTime(year, month, 1);
            var startOffset = startOfMonth.DayOfWeek == model.StartOfWeek ? 7 : (7 + startOfMonth.DayOfWeek - model.StartOfWeek) % 7;
            var startOfCalendar = startOfMonth.AddDays(-startOffset);

            for (var day = 0; day < 42; day++)
            {
                model.Days.Add(startOfCalendar.AddDays(day).Date, new Collection<Post>());
            }

            var posts = await _blogService.GetPostsAsync(post => post.Status == PostStatus.Published && post.PublishedOn.Year == year && post.PublishedOn.Month == month);
            await foreach (var post in posts)
            {
                model.Days[post.PublishedOn.Date].Add(post);
            }

            return model;
        }

        private async Task<ArchiveModel> GetYearModel(int year)
        {
            var model = new ArchiveModel { Year = year, Month = null };

            var posts = await _blogService.GetPostsAsync(post => post.Status == PostStatus.Published && post.PublishedOn.Year == year);
            await foreach (var post in posts)
            {
                if (!model.Days.Any(x => x.Key == post.PublishedOn.Date))
                {
                    model.Days.Add(post.PublishedOn.Date, new Collection<Post>());
                }

                model.Days[post.PublishedOn.Date].Add(post);
            }

            return model;
        }

        private void UpdateSeoForPost(Post post)
        {
            ViewData[Constants.PageTitle] = post.Title;
            var content = _markdownParser.ToHtml(post.Content, out var image);
            var description = !string.IsNullOrWhiteSpace(post.Excerpt)
                ? post.Excerpt
                : content;

            // TODO: Might need to take part of post.
            ViewData[Constants.Description] = description.StripHtml().Clip(87);
            ViewData[Constants.Keywords] = string.Join(", ", post.Categories.Select(c => c.Name));
            ViewData[Constants.Author] = post.Author.ShownName();
            ViewData[Constants.Image] = image;
            ViewData[Constants.Video] = null;
        }
    }
}
