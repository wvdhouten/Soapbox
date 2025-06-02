namespace Soapbox.Application.Blog.Posts.Get;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Blog;
using Soapbox.Domain.Results;

[Injectable]
public class GetPostByIdHandler
{
    private readonly IBlogRepository _blogService;

    public GetPostByIdHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<Post>> GetPostAsync(string id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post is null)
            return Error.NotFound($"Post with ID '{id}' does not exist.");

        return post;
    }
}
