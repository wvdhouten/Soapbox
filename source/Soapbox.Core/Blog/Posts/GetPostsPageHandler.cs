namespace Soapbox.Application.Blog.Posts;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;
using Soapbox.Domain.Results;

[Injectable]
public class GetPostsPageHandler
{
    private readonly IBlogRepository _blogService;

    public GetPostsPageHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<IPagedList<Post>>> GetPostsAsync(int page, int pageSize, bool isPublished = true)
    {
        var posts = await _blogService.GetPostsPageAsync(page, pageSize, isPublished);

        return Result.Success(posts);
    }
}
