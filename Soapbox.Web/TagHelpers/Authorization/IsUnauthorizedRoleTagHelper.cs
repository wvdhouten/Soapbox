namespace DasBlog.Web.TagHelpers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Soapbox.Models;

    [HtmlTargetElement(Attributes = "is-unauthorized")]
    public class IsUnauthorizedRoleTagHelper : TagHelper
	{
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<SoapboxUser> _signInManager;

        public IsUnauthorizedRoleTagHelper(IHttpContextAccessor httpContextAccessor, SignInManager<SoapboxUser> signInManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            var user = _httpContextAccessor.HttpContext.User;

            if (_signInManager.IsSignedIn(user))
            {
                output.SuppressOutput();
            }
        }
    }
}
