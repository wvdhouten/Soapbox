namespace Soapbox.Web.TagHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    public enum ActiveRoutePrecision
    {
        All,
        Action,
        Controller,
        Area
    }

    [HtmlTargetElement(Attributes = IsActiveRouteAttributeName)]
    public class ControllerActiveTagHelper : TagHelper
    {
        private const string IsActiveRouteAttributeName = "is-active-route";
        private ActiveRoutePrecision _precision;
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
        public ActiveRoutePrecision Precision
        {
            get => _precision;
            set
            {
                switch (value)
                {
                    case ActiveRoutePrecision.All:
                    case ActiveRoutePrecision.Action:
                    case ActiveRoutePrecision.Controller:
                    case ActiveRoutePrecision.Area:
                        _precision = value;
                        break;

                    default:
                        //throw new ArgumentException(message: Resources.FormatInvalidEnumArgument(nameof(value), value, typeof(ValidationSummary).FullName), paramName: nameof(value));
                        throw new Exception();
                }
            }
        }

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
            var currentArea = ViewContext.RouteData.Values["Area"].ToString();
            var currentController = ViewContext.RouteData.Values["Controller"].ToString();
            var currentAction = ViewContext.RouteData.Values["Action"].ToString();

            if (!string.IsNullOrWhiteSpace(Area) && Area.ToLower() != currentArea.ToLower())
            {
                return false;
            }

            if (Precision == ActiveRoutePrecision.Area)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(Controller) && Controller.ToLower() != currentController.ToLower())
            {
                return false;
            }

            if (Precision == ActiveRoutePrecision.Controller)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(Action) && Action.ToLower() != currentAction.ToLower())
            {
                return false;
            }

            if (Precision == ActiveRoutePrecision.Action)
            {
                return true;
            }

            foreach (var routeValue in RouteValues)
            {
                if (!ViewContext.RouteData.Values.ContainsKey(routeValue.Key) ||
                    ViewContext.RouteData.Values[routeValue.Key].ToString() != routeValue.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void MakeActive(TagHelperOutput output)
        {
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttr == null)
            {
                classAttr = new TagHelperAttribute("class", "active");
                output.Attributes.Add(classAttr);
            }
            else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf("active") < 0)
            {
                output.Attributes.SetAttribute("class", classAttr.Value == null
                    ? "active"
                    : classAttr.Value.ToString() + " active");
            }
        }
    }
}
