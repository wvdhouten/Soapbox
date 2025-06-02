namespace Soapbox.Application.Blog.Posts.Get;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

[Injectable]
public class GetPostHandler
{
    private readonly IBlogRepository _blogService;

    public GetPostHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<Post>> GetPostByIdAsync(string id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post is null)
            return Error.NotFound($"Post with ID '{id}' does not exist.");

        return post;
    }

    public async Task<Result<Post>> GetPostBySlugAsync(string slug)
    {
        var post = await _blogService.GetPostBySlugAsync(slug);
        if (post is null)
            return Error.NotFound($"Post with slug '{slug}' does not exist.");

        return post;
    }
}
