namespace Soapbox.Web.Areas.Admin.Models.Posts
{
    using System.Collections.Generic;
    using Soapbox.Models;

    public class PostViewModel : Post
    {
        public IList<PostCategoryViewModel> AllCategories { get; set; } = new List<PostCategoryViewModel>();

        public string NewCategory { get; set; }
    }
}
