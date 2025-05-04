namespace Soapbox.Web.TagHelpers.Blog;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Soapbox.Domain.Blog;

public enum ShareTarget
{
    Facebook,
    Twitter,
    Email
}

public class ShareTagHelper : TagHelper
{
    public const string FacebookUrl = "https://www.facebook.com/sharer/sharer.php?u={0}";
    public const string TwitterUrl = "https://twitter.com/intent/tweet?url={0}&text={1}";
    public const string EmailUrl = "mailto:?subject={1}&body={0}";
    private readonly LinkGenerator _linkGenerator;

    public Post Post { get; set; }

    public ShareTarget To { get; set; }

    public string Title { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public ShareTagHelper(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        var url = _linkGenerator.GetUriByAction("Post", "Blog", new { Area = "", Post.Slug }, ViewContext.HttpContext.Request.Scheme, ViewContext.HttpContext.Request.Host);
        var text = System.Net.WebUtility.UrlEncode(Post.Title);

        var href = To switch
        {
            ShareTarget.Email => string.Format(EmailUrl, url.Replace(" ", "%20;"), text),
            ShareTarget.Facebook => string.Format(FacebookUrl, url),
            ShareTarget.Twitter => string.Format(TwitterUrl, url, text),
            _ => throw new NotImplementedException(),
        };
            
        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("target", "_blank");
        output.Attributes.SetAttribute("rel", "noopener noreferrer nofollow");
        output.Attributes.SetAttribute("href", href);
        output.Attributes.SetAttribute("title", Title);

        return Task.CompletedTask;
    }
}
