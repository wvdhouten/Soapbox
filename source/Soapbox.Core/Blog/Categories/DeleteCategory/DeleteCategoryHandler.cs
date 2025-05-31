namespace Soapbox.Application.Blog.Categories.DeleteCategory;

using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using System.Threading.Tasks;

public class DeleteCategoryHandler
{
    private readonly IBlogRepository _blogRepository;

    public DeleteCategoryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result> DeleteCategoryByIdAsync(long id)
    {
        try
        {
            await _blogRepository.DeleteCategoryByIdAsync(id);
            return Result.Success();
        }
        catch
        {
            return Error.Unknown("An error occurred while deleting the category.");
        }
    }
}
