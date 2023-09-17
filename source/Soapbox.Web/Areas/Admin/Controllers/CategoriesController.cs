namespace Soapbox.Web.Areas.Admin.Controllers
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Soapbox.Core.Extensions;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.Models;
    using Soapbox.Web.Areas.Admin.Models.Categories;
    using Soapbox.Web.Controllers;
    using Soapbox.Web.Identity.Attributes;

    [Area("Admin")]
    [RoleAuthorize(UserRole.Administrator, UserRole.Editor)]
    public class CategoriesController : SoapboxBaseController
    {
        private readonly IBlogService _blogService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(IBlogService blogService, IMapper mapper, ILogger<CategoriesController> logger)
        {
            _blogService = blogService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            var model = await _blogService.GetCategoriesPageAsync(page, pageSize, true);

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new PostCategoryViewModel { GenerateSlugFromName = true };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PostCategoryViewModel category)
        {
            category.Slug = category.GenerateSlugFromName || string.IsNullOrWhiteSpace(category.Slug) ? CreateSlug(category.Name) : category.Slug;

            await _blogService.CreateCategoryAsync(category);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var category = await _blogService.GetCategoryByIdAsync(id) ?? throw new Exception("Not Found");

            var model = _mapper.Map<PostCategoryViewModel>(category);
            model.GenerateSlugFromName = category.Slug == CreateSlug(category.Name);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] PostCategoryViewModel category)
        {
            category.Slug = category.GenerateSlugFromName || string.IsNullOrWhiteSpace(category.Slug) ? CreateSlug(category.Name) : category.Slug;

            await _blogService.UpdateCategoryAsync(category);

            return RedirectToAction(nameof(Edit), new { id = category.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _blogService.DeleteCategoryByIdAsync(id);
            }
            catch
            {
                // TODO: Add status message.
            }

            return RedirectToAction(nameof(Index));
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
