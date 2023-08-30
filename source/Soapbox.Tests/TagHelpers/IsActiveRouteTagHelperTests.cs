namespace Soapbox.Tests.TagHelpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.AspNetCore.Routing;
    using Soapbox.Web.TagHelpers;

    public class IsActiveRouteTagHelperTests
    {
        [InlineData(false, ActiveRoutePrecision.Area, null, null, null)]
        [InlineData(true, ActiveRoutePrecision.Area, "Area", null, null)]
        [InlineData(false, ActiveRoutePrecision.Controller, null, null, null)]
        [InlineData(false, ActiveRoutePrecision.Controller, "Area", null, null)]
        [InlineData(true, ActiveRoutePrecision.Controller, "Area", "Controller", null)]
        [InlineData(false, ActiveRoutePrecision.Action, null, null, null)]
        [InlineData(false, ActiveRoutePrecision.Action, "Area", null, null)]
        [InlineData(false, ActiveRoutePrecision.Action, "Area", "Controller", null)]
        [InlineData(true, ActiveRoutePrecision.Action, "Area", "Controller", "Action")]
        [Theory]
        [Trait("Category", "TagHelper")]
        public async Task RecognizesActiveRoute(bool isActiveExpected, ActiveRoutePrecision precision, string area, string controller, string action)
        {
            // Arrange
            var tagHelper = new IsActiveRouteTagHelper
            {
                Precision = precision,
                Area = area,
                Controller = controller,
                Action = action,
                ViewContext = GetViewContext()
            };
            var attributeList = new TagHelperAttributeList
            {
                new TagHelperAttribute("asp-area", "Area")
            };
            var context = new TagHelperContext(attributeList, new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput("a", new TagHelperAttributeList(), (useCachedResult, htmlEncoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Same("a", output.TagName);

            var classAttribute = output.Attributes.FirstOrDefault(a => a.Name == "class");
            var isActive = classAttribute is not null
                && classAttribute.Value.ToString().Contains("active");

            Assert.Equal(isActiveExpected, isActive);
        }

        private static ViewContext GetViewContext()
        {
            var routeData = new RouteData();
            routeData.Values.Add("Area", "Area");
            routeData.Values.Add("Controller", "Controller");
            routeData.Values.Add("Action", "Action");

            var actionContext = new ActionContext(new DefaultHttpContext(), routeData, new ActionDescriptor());
            return new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                Mock.Of<ITempDataDictionary>(),
                TextWriter.Null,
                new HtmlHelperOptions());
        }
    }
}
