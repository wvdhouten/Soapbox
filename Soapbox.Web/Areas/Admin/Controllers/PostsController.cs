namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Extensions;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Posts;
    using Soapbox.Web.Identity.Attributes;
    using Soapbox.Web.Identity.Extensions;

    public class PagingOptions
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public string OrderBy { get; set; }
    }

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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 25)
        {
            var model = await _blogService.GetPostsPageAsync(page, pageSize, false);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _blogService.GetAllCategoriesAsync();
            var model = new PostViewModel
            {
                AllCategories = await categories.Select(c => _mapper.Map<SelectableCategoryViewModel>(c)).ToListAsync(),
                UpdateSlugFromTitle = true
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

            post.Author = new SoapboxUser { Id = User.GetUserId() };
            var now = DateTime.UtcNow;
            post.ModifiedOn = post.UpdateModifiedOn ? now : post.ModifiedOn;
            post.PublishedOn = post.UpdatePublishedOn ? now : post.PublishedOn;
            post.Slug = post.UpdateSlugFromTitle || string.IsNullOrWhiteSpace(post.Slug) ? CreateSlug(post.Title) : post.Slug;
            SetSelectedCategories(post);

            await _blogService.CreatePostAsync(post);

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
            model.UpdateSlugFromTitle = post.Slug == CreateSlug(post.Title);
            model.UpdatePublishedOn = model.UpdateModifiedOn = post.Status == PostStatus.Private && post.ModifiedOn == post.PublishedOn;
            model.AllCategories = await (await _blogService.GetAllCategoriesAsync()).Select(category =>
            {
                return _mapper.Map<PostCategory, SelectableCategoryViewModel>(category, opts =>
                {
                    opts.AfterMap((source, destination) => { destination.Selected = post.Categories.Any(c => destination.Id == c.Id); });
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

            var now = DateTime.UtcNow;
            post.ModifiedOn = post.UpdateModifiedOn ? now : post.ModifiedOn;
            post.PublishedOn = post.UpdatePublishedOn ? now : post.PublishedOn;
            post.Slug = post.UpdateSlugFromTitle || string.IsNullOrWhiteSpace(post.Slug) ? CreateSlug(post.Title) : post.Slug;
            SetSelectedCategories(post);

            await _blogService.UpdatePostAsync(post);

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

            var newCategory = new SelectableCategoryViewModel { Name = model.NewCategory.Trim(), Selected = true };
            if (model.AllCategories.Any(c => c.Name == newCategory.Name))
            {
                ModelState.AddModelError(nameof(model.NewCategory), $"The category '{model.NewCategory}' already exists");
            }
            else
            {
                model.AllCategories.Add(newCategory);
                model.NewCategory = string.Empty;
                ModelState.Remove(nameof(model.NewCategory));
            }

            return View(model);
        }

        private static void SetSelectedCategories(PostViewModel post)
        {
            foreach (var category in post.AllCategories.Where(c => c.Selected))
            {
                if (category.Id == default)
                {
                    category.Slug = CreateSlug(category.Name);
                }
                post.Categories.Add(category);
            }
        }

        private static string CreateSlug(string title)
        {
            title = title?.Trim().ToLowerInvariant().Replace(" ", "-", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            title = title.RemoveDiacritics();
            title = title.RemoveReservedUrlCharacters();

            return title.ToLowerInvariant();
        }
    }
}
