namespace Soapbox.Application.Blog.Categories.DeleteCategory;

using Alkaline64.Injectable;
using Soapbox.DataAccess.Abstractions;
using Soapbox.Domain.Results;
using System.Threading.Tasks;

[Injectable]
public class DeleteCategoryHandler
{
    private readonly IBlogRepository _blogRepository;

    public DeleteCategoryHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<Result> DeleteCategoryByIdAsync(string id)
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
