namespace Soapbox.Application.Blog.Categories.GetAllCategories;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

[Injectable]
public class GetAllCategoriesHandler
{
    private readonly IBlogRepository _blogService;

    public GetAllCategoriesHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<IEnumerable<PostCategory>>> GetAllCategoriesAsync()
    {
        var categories = await _blogService.GetAllCategoriesAsync(true);

        return Result.Success(categories);
    }
}
