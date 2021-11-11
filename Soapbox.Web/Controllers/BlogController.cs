namespace Soapbox.Web.Controllers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Soapbox.Core.Common;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Models.Blog;

    [Route("blog")]
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet("{page:int?}")]
        public async Task<IActionResult> Index(int page = 0)
        {
            var posts = await _blogService.GetPostsPageAsync(page, 5);

            ViewData[Constants.Title] = "Blog";

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

            ViewData[Constants.Title] = post.Title;

            return View(post);
        }

        [HttpGet("post/{id}")]
        public async Task<IActionResult> PostById(string id)
        {
            var post = await _blogService.GetPostByIdAsync(id);
            if (post is null)
            {
                return NotFound();
            }

            ViewData[Constants.Title] = post.Title;

            return View(nameof(Post), post);
        }

        [HttpGet("archive")]
        [HttpGet("archive/{year:int?}")]
        [HttpGet("archive/{year:int?}/{month:int?}")]
        public async Task<IActionResult> Archive(int year = 0, int month = 0)
        {
            year = year > 0 ? year : DateTime.Now.Year;
            month = month > 0 ? month : DateTime.Now.Month;

            var currentDate = new DateTime(year, month, 1);
            var model = await GetMonthModel(currentDate);

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
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpGet("author/{id}")]
        public async Task<IActionResult> Author(string id)
        {
            var author = await _blogService.GetAuthorByIdAsync(id);

            return View(author);
        }

        private async Task<ArchiveModel> GetMonthModel(DateTime currentMonth)
        {
            var model = new ArchiveModel { CurrentMonth = currentMonth };
            var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var startOffset = startOfMonth.DayOfWeek == model.StartOfWeek ? 7 : (7 + startOfMonth.DayOfWeek - model.StartOfWeek) % 7;
            var startOfCalendar = startOfMonth.AddDays(-startOffset);

            for (var day = 0; day < 42; day++)
            {
                model.Days.Add(startOfCalendar.AddDays(day).Date, new Collection<Post>());
            }

            var posts = await _blogService.GetPostsAsync(post => post.PublishedOn.HasValue && post.PublishedOn.Value.Year == currentMonth.Year && post.PublishedOn.Value.Month == currentMonth.Month);
            await foreach (var post in posts)
            {
                model.Days[post.PublishedOn.Value.Date].Add(post);
            }

            return model;
        }
    }
}
