namespace Soapbox.Application.Blog.Categories.UpdateCategory;

using Alkaline64.Injectable;
using Soapbox.Application.Utils;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;

[Injectable]
public class UpdateCategoryHandler
{
    private readonly IBlogRepository _blogRepository;

    public UpdateCategoryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result> UpdateCategoryAsync(UpdateCategoryRequest request)
    {
        if (request.GenerateSlugFromName || string.IsNullOrWhiteSpace(request.Category.Slug))
            request.Category.Slug = Slugifier.Slugify(request.Category.Name);

        await _blogRepository.UpdateCategoryAsync(request.Category);

        return Result.Success();
    }
}
