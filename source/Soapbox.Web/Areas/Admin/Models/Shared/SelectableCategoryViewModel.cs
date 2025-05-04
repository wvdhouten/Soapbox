namespace Soapbox.Web.Areas.Admin.Models.Shared;

using Soapbox.Domain.Blog;

public class SelectableItemViewModel<TItem>
{
    public bool Selected { get; set; }

    public TItem Item { get; set; } = default!;
}
