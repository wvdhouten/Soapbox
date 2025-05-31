namespace Soapbox.Application.Blog.Categories.ListCategories;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;
using Soapbox.Domain.Results;

[Injectable]
public class ListCategoriesHandler
{
    private readonly IBlogRepository _blogRepository;

    public ListCategoriesHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result<IEnumerable<PostCategory>>> GetAllCategoriesAsync()
    {
        var categories = await _blogRepository.GetAllCategoriesAsync(true);

        return Result.Success(categories);
    }

    public async Task<Result<IPagedList<PostCategory>>> GetCategoryPageAsync(int page = 1, int pageSize = 25, bool includePosts = true)
    {
        var result = await _blogRepository.GetCategoriesPageAsync(page, pageSize, includePosts);

        return Result.Success(result);
    }
}
