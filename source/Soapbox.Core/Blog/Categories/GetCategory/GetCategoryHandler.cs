namespace Soapbox.Application.Blog.Categories.GetCategory;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

[Injectable]
public class GetCategoryHandler
{
    private readonly IBlogRepository _blogService;

    public GetCategoryHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<PostCategory>> GetCategoryByIdAsync(long id)
    {
        var category = await _blogService.GetCategoryByIdAsync(id);
        if (category is null)
            return Error.NotFound($"Post with id '{id}' does not exist.");

        return category;
    }

    public async Task<Result<PostCategory>> GetCategoryBySlugAsync(string slug)
    {
        var category = await _blogService.GetCategoryBySlugAsync(slug);
        if (category is null)
            return Error.NotFound($"Post with slug '{slug}' does not exist.");

        return category;
    }
}
