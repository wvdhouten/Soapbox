namespace Soapbox.Web.Areas.Admin.Models.Posts
{
    using Soapbox.Models;

    public class SelectableCategoryViewModel : PostCategory
    {
        public bool Selected { get; set; }
    }
}
