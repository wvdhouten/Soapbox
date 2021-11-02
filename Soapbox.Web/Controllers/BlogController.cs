namespace Soapbox.Web.Controllers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Soapbox.Core.Common;
    using Soapbox.Domain.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Models.Blog;

    [Route("blog")]
    public class BlogController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IBlogService _blogService;

        public BlogController(IAuthorizationService authorizationService, IBlogService blogService)
        {
            _authorizationService = authorizationService;
            _blogService = blogService;
        }

        [HttpGet("{page:int?}")]
        public async Task<IActionResult> Index(int page = 0)
        {
            var posts = await _blogService.GetPostsAsync(5, page);

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

        [HttpGet("author/{id}")]
        public async Task<IActionResult> Author(string id)
        {
            // var author = 
            //if (author is null)
            //{
            //    return NotFound();
            //}
            var posts = await _blogService.GetPostsByAuthorAsync(id);

            return View(posts);
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
