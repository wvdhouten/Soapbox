namespace Soapbox.Application.Blog.Posts.Delete;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using System.Threading.Tasks;

public class DeletePostHandler
{
    private readonly IBlogRepository _blogRepository;

    public DeletePostHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result> DeletePostAsync(string id)
    {
        try
        {
            await _blogRepository.DeletePostByIdAsync(id);
            return Result.Success();
        }
        catch
        {
            return Error.Unknown();
        }
    }
}
