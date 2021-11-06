namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Extensions;
    using Soapbox.Domain.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Posts;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator, UserRole.Editor, UserRole.Author, UserRole.Contributor)]
    public class PostsController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly IMapper _mapper;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IBlogService blogService, IMapper mapper, ILogger<PostsController> logger)
        {
            _blogService = blogService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _blogService.GetAllPostsAsync();

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PostViewModel
            {
                AllCategories = await (await _blogService.GetAllCategoriesAsync()).Select(category => _mapper.Map<PostCategoryViewModel>(category)).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PostViewModel post, string action)
        {
            if (action == nameof(AddCategory))
            {
                return AddCategory(post);
            }

            if (!ModelState.IsValid)
            {
                return View(post);
            }

            post.Author.Id = User.GetUserId<string>();
            foreach (var category in post.AllCategories.Where(c => c.Checked))
            {
                if (category.Id == default)
                {
                    category.Slug = CreateSlug(category.Name);
                }
                post.Categories.Add(category);
            }

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

            var model = _mapper.Map<PostViewModel>(post);

            model.AllCategories = await (await _blogService.GetAllCategoriesAsync()).Select(category =>
            {
                return _mapper.Map<PostCategory, PostCategoryViewModel>(category, opts =>
                {
                    opts.AfterMap((source, destination) => { destination.Checked = post.Categories.Any(c => destination.Id == c.Id); });
                });
            }).ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] PostViewModel post, string action)
        {
            if (action == nameof(AddCategory))
            {
                return AddCategory(post);
            }

            if (!ModelState.IsValid)
            {
                return View(post);
            }

            foreach (var category in post.AllCategories.Where(c => c.Checked))
            {
                if (category.Id == default)
                {
                    category.Slug = CreateSlug(category.Name);
                }

                post.Categories.Add(category);
            }

            await _blogService.CreateOrUpdatePostAsync(post);

            return RedirectToAction(nameof(Edit), new { id = post.Id });
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

        private IActionResult AddCategory(PostViewModel model)
        {
            ModelState.ClearValidationState(string.Empty);
            if (string.IsNullOrWhiteSpace(model.NewCategory))
            {
                ModelState.AddModelError(nameof(model.NewCategory), "To add a category you must enter some text in the box next to the 'Add' button before clicking 'Add'");
                return View(model);
            }

            var categoryName = model.NewCategory.Trim();
            var newCategory = new PostCategoryViewModel
            {
                Name = categoryName,
                Checked = true
            };

            if (model.AllCategories.Any(c => c.Name == newCategory.Name))
            {
                ModelState.AddModelError(nameof(model.NewCategory), $"The category \"{model.NewCategory}\" already exists");
            }
            else
            {
                model.AllCategories.Add(newCategory);
                model.NewCategory = string.Empty;
                ModelState.Remove(nameof(model.NewCategory));
            }

            return View(model);
        }

        private static string CreateSlug(string title)
        {
            title = title?.ToLowerInvariant().Replace(" ", "-", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            title = title.RemoveDiacritics();
            title = title.RemoveReservedUrlCharacters();

            return title.ToLowerInvariant();
        }
    }
}
