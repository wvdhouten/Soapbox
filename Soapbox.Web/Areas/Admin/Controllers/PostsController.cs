namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Identity;
    using Soapbox.Domain.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator, UserRole.Editor, UserRole.Author, UserRole.Contributor)]
    public class PostsController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IBlogService blogService, ILogger<PostsController> logger)
        {
            _blogService = blogService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromRoute] int page = 0)
        {
            var posts = await _blogService.GetAllPostsAsync();

            return View(posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Post post)
        {
            if (!ModelState.IsValid)
            {
                return View(post);
            }

            post.Author = User.GetUserId<string>();
            await _blogService.CreateOrUpdatePostAsync(post);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var post = await _blogService.GetPostByIdAsync(id);
            if (post is null)
            {
                throw new Exception("Not Found");
            }    

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] Post post)
        {
            if (!ModelState.IsValid)
            {
                return View(post);
            }

            await _blogService.CreateOrUpdatePostAsync(post);

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _blogService.DeletePostByIdAsync(id);
            }
            catch
            {
                // TODO: Add status message.
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
