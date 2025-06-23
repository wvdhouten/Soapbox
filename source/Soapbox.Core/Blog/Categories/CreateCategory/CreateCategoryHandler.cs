namespace Soapbox.Application.Blog.Categories.CreateCategory;

using Alkaline64.Injectable;
using Soapbox.Application.Utils;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using System.Threading.Tasks;

[Injectable]
public class CreateCategoryHandler
{
    public const string DuplicateCategory = nameof(DuplicateCategory);

    private readonly IBlogRepository _blogRepository;

    public CreateCategoryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if (request.GenerateSlugFromName || string.IsNullOrWhiteSpace(request.Category.Slug))
            request.Category.Slug = Slugifier.Slugify(request.Category.Name);

        var existing = await _blogRepository.GetCategoryBySlugAsync(request.Category.Slug);
        if (existing is not null)
            return Error.Other(DuplicateCategory);

        await _blogRepository.CreateCategoryAsync(request.Category);

        return Result.Success();
    }
}
