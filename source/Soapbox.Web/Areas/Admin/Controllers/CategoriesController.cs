namespace Soapbox.Web.Areas.Admin.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Soapbox.Application.Extensions;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Users;
using Soapbox.Web.Areas.Admin.Models.Categories;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Identity.Attributes;

[Area("Admin")]
[RoleAuthorize(UserRole.Administrator, UserRole.Editor)]
public class CategoriesController : SoapboxControllerBase
{
    private readonly IBlogRepository _blogService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IBlogRepository blogService, ILogger<CategoriesController> logger)
    {
        _blogService = blogService;
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
    public async Task<IActionResult> Create([FromForm] PostCategoryViewModel model)
    {
        model.Category.Slug = model.GenerateSlugFromName || string.IsNullOrWhiteSpace(model.Category.Slug) ? CreateSlug(model.Category.Name) : model.Category.Slug;

        await _blogService.CreateCategoryAsync(model.Category);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var category = await _blogService.GetCategoryByIdAsync(id) ?? throw new Exception("Not Found");

        var model = new PostCategoryViewModel
        {
            Category = category,
            GenerateSlugFromName = category.Slug == CreateSlug(category.Name)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromForm] PostCategoryViewModel model)
    {
        model.Category.Slug = model.GenerateSlugFromName || string.IsNullOrWhiteSpace(model.Category.Slug) ? CreateSlug(model.Category.Name) : model.Category.Slug;

        await _blogService.UpdateCategoryAsync(model.Category);

        return RedirectToAction(nameof(Edit), new { id = model.Category.Id });
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
