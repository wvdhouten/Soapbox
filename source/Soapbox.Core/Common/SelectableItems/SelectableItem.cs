namespace Soapbox.Application.Common.SelectableItems;

public class SelectableItem<TItem>
{
    public bool Selected { get; set; }

    public TItem Item { get; set; } = default!;
}
