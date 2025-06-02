namespace Soapbox.Application.Blog.Posts.Edit;

using Soapbox.Domain.Blog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class EditPostRequest
{
    public Post Post { get; set; } = default!;

    [Display(Name = "Update from title")]
    public bool UpdateSlugFromTitle { get; set; } = true;

    [Display(Name = "Update")]
    public bool UpdateModifiedOn { get; set; } = true;

    [Display(Name = "Update")]
    public bool UpdatePublishedOn { get; set; } = true;

    public IEnumerable<string> SelectedCategories { get; set; } = [];

    public string? NewCategory { get; set; }
}
