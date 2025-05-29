namespace Soapbox.Application.Blog.Posts;
using System.Threading.Tasks;
using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

[Injectable]
public class GetPostBySlugHandler
{
    private readonly IBlogRepository _blogService;

    public GetPostBySlugHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<Post>> GetPostAsync(string slug)
    {
        var post = await _blogService.GetPostBySlugAsync(slug);
        if (post is null)
            return Error.NotFound($"Post with slug '{slug}' does not exist.");

        return post;
    }
}
