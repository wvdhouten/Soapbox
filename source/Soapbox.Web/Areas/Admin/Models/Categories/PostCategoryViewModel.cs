namespace Soapbox.Web.Areas.Admin.Models.Categories
{
    using Soapbox.Models;

    public class PostCategoryViewModel : PostCategory
    {
        public bool GenerateSlugFromName { get; set; }
    }
}
