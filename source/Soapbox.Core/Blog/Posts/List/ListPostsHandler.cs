namespace Soapbox.Application.Blog.Posts.List;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Common;
using Soapbox.Domain.Results;

[Injectable]
public class ListPostsHandler
{
    private readonly IBlogRepository _blogService;

    public ListPostsHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<IPagedList<Post>>> GetPostsPage(int page = 1, int pageSize = 25, bool isPublished = true)
    {
        var posts = await _blogService.GetPostsPageAsync(page, pageSize, isPublished);

        return Result.Success(posts);
    }
}
