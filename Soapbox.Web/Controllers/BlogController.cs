namespace Soapbox.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Soapbox.Core.Common;
    using Soapbox.Domain.Abstractions;
    using Soapbox.Models;

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
            var posts = await _blogService.GetAllPostsAsync();

            //// apply paging filter.
            //var filteredPosts = posts.Skip(this.settings.Value.PostsPerPage * page).Take(this.settings.Value.PostsPerPage);

            //// set the view option
            //this.ViewData[Constants.ViewOption] = this.settings.Value.ListView;

            //this.ViewData[Constants.TotalPostCount] = await posts.CountAsync().ConfigureAwait(true);
            ViewData[Constants.Title] = "Blog";
            //this.ViewData[Constants.Description] = this.manifest.Description;
            //this.ViewData[Constants.prev] = $"/{page + 1}/";
            //this.ViewData[Constants.next] = $"/{(page <= 1 ? null : $"{page - 1}/")}";

            return View(posts);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Read(string slug)
        {
            var post = await _blogService.GetPostBySlugAsync(slug);

            ViewData[Constants.Title] = post;

            return View(post);
        }

        [HttpGet("{id?}/edit")]
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(new Post());
            }

            var post = await _blogService.GetPostByIdAsync(id);
            if (post is null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, post, "EditPostPolicy");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost("{id?}")]
        [Authorize, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(Post post)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), post);
            }

            if (post is null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            await _blogService.CreateOrUpdatePostAsync(post);

            return View(post);

            //return this.Redirect(post.GetEncodedLink());
        }
    }
}
