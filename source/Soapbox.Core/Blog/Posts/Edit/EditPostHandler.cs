namespace Soapbox.Application.Blog.Posts.Edit;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Soapbox.Application.Utils;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Web.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

[Injectable]
public class EditPostHandler
{
    private readonly IBlogRepository _blogRepository;
    private readonly IHttpContextAccessor _contextAccessor;

    public EditPostHandler(IBlogRepository blogRepository, IHttpContextAccessor contextAccessor)
    {
        _blogRepository = blogRepository;
        _contextAccessor = contextAccessor;
    }

    public async Task<Result> QuickAddCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return Error.InvalidOperation("Category cannot be null.");

        await _blogRepository.CreateCategoryAsync(new PostCategory
        {
            Name = category ?? throw new ArgumentNullException(nameof(category)),
            Slug = Slugifier.Slugify(category)
        });

        return Result.Success();
    }

    public async Task<Result> UpdatePost(EditPostRequest request)
    {
        request.Post.Author = new SoapboxUser { Id = _contextAccessor.HttpContext.User.GetUserId() };
        var now = DateTime.UtcNow;
        request.Post.ModifiedOn = request.UpdateModifiedOn ? now : request.Post.ModifiedOn;
        request.Post.PublishedOn = request.UpdatePublishedOn ? now : request.Post.PublishedOn;
        request.Post.Slug = request.UpdateSlugFromTitle || string.IsNullOrWhiteSpace(request.Post.Slug) ? Slugifier.Slugify(request.Post.Title) : request.Post.Slug;
        request.Post.Categories = [.. request.SelectedCategories.Select(c => new PostCategory { Id = c })];

        await _blogRepository.UpdatePostAsync(request.Post);

        return Result.Success();
    }
}
