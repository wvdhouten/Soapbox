﻿namespace Soapbox.Application.Blog.Posts.Create;

using Soapbox.Domain.Blog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreatePostRequest : ICanAddCategory
{
    public Post Post { get; set; } = new();

    [Display(Name = "Update from title")]
    public bool UpdateSlugFromTitle { get; set; } = true;

    [Display(Name = "Update")]
    public bool UpdateModifiedOn { get; set; } = true;

    [Display(Name = "Update")]
    public bool UpdatePublishedOn { get; set; } = true;

    [Display(Name = "Categories")]
    public IList<string> SelectedCategories { get; set; } = [];

    public string? NewCategory { get; set; }

    public string? Action { get; set; }
}
