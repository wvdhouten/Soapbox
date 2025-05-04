namespace Soapbox.Web.Areas.Admin.Models.Categories
{
    using Soapbox.Domain.Blog;

    public class PostCategoryViewModel
    {
        public bool GenerateSlugFromName { get; set; }

        public PostCategory Category { get; set; }
    }
}
