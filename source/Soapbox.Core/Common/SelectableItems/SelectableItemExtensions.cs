namespace Soapbox.Application.Common.SelectableItems;
public static class SelectableItemExtensions
{
    public static IEnumerable<SelectableItem<TItem>> ToSelectableItems<TItem>(this IEnumerable<TItem> enumerable) 
        => enumerable.Select(item => new SelectableItem<TItem> { Item = item, Selected = false });

    public static IEnumerable<SelectableItem<TItem>> ToSelectableItems<TItem>(this IEnumerable<TItem> enumerable, Func<TItem, bool> isSelected)
        => enumerable.Select(item => new SelectableItem<TItem> { Item = item, Selected = isSelected(item) });
}
