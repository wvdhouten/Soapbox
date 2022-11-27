namespace Soapbox.Web.Areas.Admin.Models.Posts
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Soapbox.Models;

    public class PostViewModel : Post
    {
        public IList<SelectableCategoryViewModel> AllCategories { get; set; } = new List<SelectableCategoryViewModel>();

        [Display(Name = "Update from title")]
        public bool UpdateSlugFromTitle { get; set; } = true;

        [Display(Name = "Update")]
        public bool UpdateModifiedOn { get; set; } = true;

        [Display(Name = "Update")]
        public bool UpdatePublishedOn { get; set; } = true;

        public string NewCategory { get; set; }
    }
}
