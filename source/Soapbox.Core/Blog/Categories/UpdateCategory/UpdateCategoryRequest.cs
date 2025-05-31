namespace Soapbox.Application.Blog.Categories.UpdateCategory;

using Soapbox.Application.Utils;
using Soapbox.Domain.Blog;

public class UpdateCategoryRequest
{
    public bool GenerateSlugFromName { get; set; } = true;

    public PostCategory Category { get; set; } = default!;

    public static UpdateCategoryRequest FromCategory(PostCategory category) =>
        new() {
            Category = category,
            GenerateSlugFromName = category.Slug == Slugifier.Sluggify(category.Name)
        };
}
