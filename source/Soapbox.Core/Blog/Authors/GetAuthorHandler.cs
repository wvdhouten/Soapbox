namespace Soapbox.Application.Blog.Authors;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;

[Injectable]
public class GetAuthorHandler
{
    private readonly IBlogRepository _blogService;

    public GetAuthorHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<SoapboxUser>> GetAuthorByIdAsync(string id)
    {
        var author = await _blogService.GetAuthorByIdAsync(id);
        if (author is null)
            return Error.NotFound($"Post with ID '{id}' does not exist.");

        return author;
    }
}
