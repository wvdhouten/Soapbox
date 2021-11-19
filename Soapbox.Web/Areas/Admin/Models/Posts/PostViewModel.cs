namespace Soapbox.Web.Areas.Admin.Models.Posts
{
    using System.Collections.Generic;
    using Soapbox.Models;

    public class PostViewModel : Post
    {
        public IList<SelectableCategoryViewModel> AllCategories { get; set; } = new List<SelectableCategoryViewModel>();

        public bool GenerateSlugFromTitle { get; set; }

        public string NewCategory { get; set; }
    }
}
