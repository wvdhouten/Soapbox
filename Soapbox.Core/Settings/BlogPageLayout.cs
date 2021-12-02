namespace Soapbox.Core.Settings
{
    using System.ComponentModel.DataAnnotations;

    public enum BlogPageLayout
    {
        [Display(Name = "List (Posts)")]
        PostList,

        [Display(Name = "List (Excerpts)")]
        ExcerptList,

        [Display(Name = "Cards (Excerpts)")]
        ExcerptCards
    }
}
