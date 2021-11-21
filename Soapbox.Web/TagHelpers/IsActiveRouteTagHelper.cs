namespace Soapbox.Web.TagHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    // TODO: Documentation
    public enum ActiveRoutePrecision
    {
        All,
        Action,
        Controller,
        Area
    }

    [HtmlTargetElement(Attributes = IsActiveRouteAttributeName)]
    public class IsActiveRouteTagHelper : TagHelper
    {
        private const string IsActiveRouteAttributeName = "is-active-route";
        private IDictionary<string, string> _routeValues;

        /// <summary>The name of the action method.</summary>
        /// <remarks>Must be <c>null</c> if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper.Route" /> is non-<c>null</c>.</remarks>
        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        /// <summary>The name of the controller.</summary>
        /// <remarks>Must be <c>null</c> if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper.Route" /> is non-<c>null</c>.</remarks>
        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        /// <summary>The name of the area.</summary>
        /// <remarks>Must be <c>null</c> if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper.Route" /> is non-<c>null</c>.</remarks>
        [HtmlAttributeName("asp-area")]
        public string Area { get; set; }

        /// <summary>Additional parameters for the route.</summary>
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                if (_routeValues == null)
                {
                    _routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return _routeValues;
            }
            set => _routeValues = value;
        }

        [HtmlAttributeName(IsActiveRouteAttributeName)]
        public ActiveRoutePrecision Precision { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ShouldBeActive())
            {
                MakeActive(output);
            }

            output.Attributes.RemoveAll(IsActiveRouteAttributeName);
        }

        private bool ShouldBeActive()
        {
            var desiredArea = Area ?? string.Empty;
            var currentArea = ViewContext.RouteData.Values["Area"]?.ToString() ?? string.Empty;
            var desiredController = Controller ?? string.Empty;
            var currentController = ViewContext.RouteData.Values["Controller"].ToString();
            var desiredAction = Action ?? string.Empty;
            var currentAction = ViewContext.RouteData.Values["Action"].ToString();

            // TODO: Add Route values.
            return Precision switch
            {
                ActiveRoutePrecision.Area => currentArea == desiredArea,
                ActiveRoutePrecision.Controller => currentArea == desiredArea
                    && currentController == desiredController,
                ActiveRoutePrecision.Action => currentArea == desiredArea
                    && currentController == desiredController
                    && currentAction == desiredAction,
                ActiveRoutePrecision.All
                or _ => currentArea == desiredArea
                    && currentController == desiredController
                    && currentAction == desiredAction,
            };
        }

        private static void MakeActive(TagHelperOutput output)
        {
            var classAttribute = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttribute == null)
            {
                classAttribute = new TagHelperAttribute("class", "active");
                output.Attributes.Add(classAttribute);
            }
            else if (classAttribute.Value == null || classAttribute.Value.ToString().IndexOf("active") < 0)
            {
                output.Attributes.SetAttribute("class", classAttribute.Value == null
                    ? "active"
                    : classAttribute.Value.ToString() + " active");
            }
        }
    }
}
