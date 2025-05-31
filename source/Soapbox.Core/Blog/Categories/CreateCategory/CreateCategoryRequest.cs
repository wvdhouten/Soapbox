namespace Soapbox.Application.Blog.Categories.CreateCategory;
using Soapbox.Domain.Blog;

public class CreateCategoryRequest
{
    public bool GenerateSlugFromName { get; set; } = true;

    public PostCategory Category { get; set; } = default!;
}
