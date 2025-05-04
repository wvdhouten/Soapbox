namespace Soapbox.Web.TagHelpers.Account;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Soapbox.Domain.Users;

[HtmlTargetElement(Attributes = IsSignedInAttribute)]
public class IsSignedInTagHelper : TagHelper
{
    private const string IsSignedInAttribute = "is-signed-in";

    public bool IsSignedIn { get; set; } = true;

    private readonly SignInManager<SoapboxUser> _signInManager;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    public IsSignedInTagHelper(SignInManager<SoapboxUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        var user = ViewContext.HttpContext.User;

        if (_signInManager.IsSignedIn(user) != IsSignedIn)
            output.SuppressOutput();

        output.Attributes.RemoveAll(IsSignedInAttribute);
    }
}
