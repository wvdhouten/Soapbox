namespace Soapbox.Application.Blog.Categories.CreateCategory;

using Soapbox.Application.Utils;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using System.Threading.Tasks;

public class CreateCategoryHandler
{
    private readonly IBlogRepository _blogRepository;

    public CreateCategoryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if (request.GenerateSlugFromName || string.IsNullOrWhiteSpace(request.Category.Slug))
            request.Category.Slug = Slugifier.Sluggify(request.Category.Name);

        await _blogRepository.CreateCategoryAsync(request.Category);

        return Result.Success();
    }
}
