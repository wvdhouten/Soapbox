namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Application.Extensions;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Domain.Blog;
    using Soapbox.Domain.Users;
    using Soapbox.Web.Areas.Admin.Models.Posts;
    using Soapbox.Web.Areas.Admin.Models.Shared;
    using Soapbox.Web.Controllers.Base;
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
    public class PostsController : SoapboxControllerBase
    {
        private readonly IBlogRepository _blogService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IBlogRepository blogService, ILogger<PostsController> logger)
        {
            _blogService = blogService;
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
                Post = new Post(),
                AllCategories = [.. categories.Select(c => new SelectableItemViewModel<PostCategory> { Item = c })],
                UpdateSlugFromTitle = true
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PostViewModel model, string action)
        {
            if (action == nameof(AddCategory))
            {
                return AddCategory(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Post.Author = new SoapboxUser { Id = User.GetUserId() };
            var now = DateTime.UtcNow;
            model.Post.ModifiedOn = model.UpdateModifiedOn ? now : model.Post.ModifiedOn;
            model.Post.PublishedOn = model.UpdatePublishedOn ? now : model.Post.PublishedOn;
            model.Post.Slug = model.UpdateSlugFromTitle || string.IsNullOrWhiteSpace(model.Post.Slug) ? CreateSlug(model.Post.Title) : model.Post.Slug;
            SetSelectedCategories(model);

            await _blogService.CreatePostAsync(model.Post);

            StatusMessage = "Post created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var post = await _blogService.GetPostByIdAsync(id) ?? throw new Exception("Not Found");

            var model = new PostViewModel
            {
                Post = post,
                UpdateSlugFromTitle = post.Slug == CreateSlug(post.Title),
                UpdatePublishedOn = post.Status == PostStatus.Draft && post.ModifiedOn == post.PublishedOn,
                UpdateModifiedOn = post.Status == PostStatus.Draft && post.ModifiedOn == post.PublishedOn,
                AllCategories = [.. (await _blogService.GetAllCategoriesAsync()).Select(category =>
                    {
                        return new SelectableItemViewModel<PostCategory> { Item = category, Selected = post.Categories.Any(c => category.Id == c.Id) };
                    })]
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] PostViewModel model, string action)
        {
            if (action == nameof(AddCategory))
            {
                return AddCategory(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var now = DateTime.UtcNow;
            model.Post.ModifiedOn = model.UpdateModifiedOn ? now : model.Post.ModifiedOn;
            model.Post.PublishedOn = model.UpdatePublishedOn ? now : model.Post.PublishedOn;
            model.Post.Slug = model.UpdateSlugFromTitle || string.IsNullOrWhiteSpace(model.Post.Slug) ? CreateSlug(model.Post.Title) : model.Post.Slug;
            SetSelectedCategories(model);

            await _blogService.UpdatePostAsync(model.Post);

            StatusMessage = "Post updated successfully.";

            return RedirectToAction(nameof(Edit), new { id = model.Post.Id });
        }

        [HttpGet, ActionName(nameof(Delete))]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            var model = await _blogService.GetPostByIdAsync(id);

            return View(nameof(Delete), model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _blogService.DeletePostByIdAsync(id);

                StatusMessage = "Post deleted successfully.";
            }
            catch
            {
                ErrorMessage = " Failed to delete post.";
            }

            return RedirectToAction(nameof(Index));
        }

        private ViewResult AddCategory(PostViewModel model)
        {
            ModelState.ClearValidationState(string.Empty);
            if (string.IsNullOrWhiteSpace(model.NewCategory))
            {
                ModelState.AddModelError(nameof(model.NewCategory), "To add a category you must enter some text in the box next to the 'Add' button before clicking 'Add'");
                return View(model);
            }

            var newCategory = new SelectableItemViewModel<PostCategory> { Item = new PostCategory { Name = model.NewCategory.Trim() }, Selected = true };
            if (model.AllCategories.Any(c => c.Item.Name == newCategory.Item.Name))
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

        private static void SetSelectedCategories(PostViewModel model)
        {
            foreach (var category in model.AllCategories.Where(c => c.Selected))
            {
                if (category.Item.Id == default)
                {
                    category.Item.Slug = CreateSlug(category.Item.Name);
                }
                model.Post.Categories.Add(category.Item);
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
