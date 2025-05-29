namespace Soapbox.Application.Blog.Authors;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;

[Injectable]
public class GetAuthorByIdHandler
{
    private readonly IBlogRepository _blogService;

    public GetAuthorByIdHandler(IBlogRepository blogService)
    {
        _blogService = blogService;
    }

    public async Task<Result<SoapboxUser>> GetAuthorAsync(string id)
    {
        var author = await _blogService.GetAuthorByIdAsync(id);
        if (author is null)
            return Error.NotFound($"Post with ID '{id}' does not exist.");

        return author;
    }
}
