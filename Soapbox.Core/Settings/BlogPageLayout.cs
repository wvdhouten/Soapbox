namespace Soapbox.Core.Settings
{
    using System.ComponentModel.DataAnnotations;

    public enum BlogPageLayout
    {
        [Display(Name = "Posts")]
        Posts,

        [Display(Name = "Excerpts")]
        Excerpts,

        [Display(Name = "Cards")]
        Cards
    }
}
