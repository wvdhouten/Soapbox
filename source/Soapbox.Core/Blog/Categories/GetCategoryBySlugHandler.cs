namespace Soapbox.Application.Blog.Categories;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

[Injectable]
public class GetCategoryBySlugHandler
{
    private readonly IBlogRepository _blogService;

    public GetCategoryBySlugHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<PostCategory>> GetCategoryAsync(string slug)
    {
        var category = await _blogService.GetCategoryBySlugAsync(slug);
        if (category is null)
            return Error.NotFound($"Post with slug '{slug}' does not exist.");

        return category;
    }
}
