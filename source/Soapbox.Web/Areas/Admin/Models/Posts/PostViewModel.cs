namespace Soapbox.Web.Areas.Admin.Models.Posts;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Soapbox.Domain.Blog;
using Soapbox.Web.Areas.Admin.Models.Shared;

public class PostViewModel
{
    public Post Post { get; set; } = default!;

    public IList<SelectableItemViewModel<PostCategory>> AllCategories { get; set; } = [];

    [Display(Name = "Update from title")]
    public bool UpdateSlugFromTitle { get; set; } = true;

    [Display(Name = "Update")]
    public bool UpdateModifiedOn { get; set; } = true;

    [Display(Name = "Update")]
    public bool UpdatePublishedOn { get; set; } = true;

    public string? NewCategory { get; set; }
}
