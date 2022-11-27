namespace Soapbox.Web.TagHelpers.Blog
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.TagHelpers;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Soapbox.Core.Extensions;
    using Soapbox.Models;

    public class BlogLinkTagHelper : TagHelper
    {
        private readonly IHtmlGenerator _generator;

        public bool Index { get; set; }

        public int? Page { get; set; }

        public Post Post { get; set; }

        public bool Categories { get; set; }

        public PostCategory Category { get; set; }

        public SoapboxUser Author { get; set; }

        public bool Archive { get; set; }

        public int? ArchiveYear { get; set; }

        public int? ArchiveMonth { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public BlogLinkTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = "a";
            output.TagMode = TagMode.StartTagAndEndTag;

            var (builder, defaultContent) = PrepareOutput();

            var childContent = await output.GetChildContentAsync();
            if (childContent.IsEmptyOrWhiteSpace)
            {
                var content = defaultContent();
                output.Content.SetContent(content);
            }

            output.MergeAttributes(builder);
        }

        private (TagBuilder builder, Func<string> defaultContent) PrepareOutput()
        {
            TagBuilder builder = null;
            Func<string> defaultContent = null;

            if (Index)
            {
                builder = GetTagBuilder("Index", null);
                defaultContent = () => "Blog";
            }

            if (Page.HasValue)
            {
                builder = GetTagBuilder("Index", new Dictionary<string, object> { { "page", Page.Value } });
                defaultContent = () => $"Page {Page.Value}";
            }

            if (Post is not null)
            {
                if (builder is not null)
                {
                    throw new InvalidOperationException("Ambiguous blog link target.");
                }

                builder = GetTagBuilder("Post", new Dictionary<string, object> { { "slug", Post.Slug } });
                defaultContent = () => Post.Title;
            }

            if (Categories)
            {
                if (builder is not null)
                {
                    throw new InvalidOperationException("Ambiguous blog link target.");
                }

                builder = GetTagBuilder("Categories", null);
                defaultContent = () => "Categories";
            }

            if (Category is not null)
            {
                if (builder is not null)
                {
                    throw new InvalidOperationException("Ambiguous blog link target.");
                }

                builder = GetTagBuilder("Category", new Dictionary<string, object> { { "slug", Category.Slug } });
                defaultContent = () => Category.Name;
            }

            if (Author is not null)
            {
                if (builder is not null)
                {
                    throw new InvalidOperationException("Ambiguous blog link target.");
                }

                builder = GetTagBuilder("Author", new Dictionary<string, object> { { "id", Author.Id } });
                defaultContent = () => Author.ShownName();
            }

            if (Archive || ArchiveYear.HasValue || ArchiveMonth.HasValue)
            {
                if (builder is not null)
                {
                    throw new InvalidOperationException("Ambiguous blog link target.");
                }

                var routeValues = new Dictionary<string, object>();
                if (ArchiveYear.HasValue)
                {
                    routeValues.Add("year", ArchiveYear.Value);
                }
                if (ArchiveMonth.HasValue)
                {
                    routeValues.Add("month", ArchiveMonth.Value);
                }

                builder = GetTagBuilder("Archive", routeValues);
                defaultContent = () => "Archive";
            }

            if (builder is null || defaultContent is null)
            {
                throw new InvalidOperationException("Cannot determine blog link target.");
            }

            return (builder, defaultContent);
        }

        private TagBuilder GetTagBuilder(string action, IDictionary<string, object> routeValues)
        {
            routeValues ??= new Dictionary<string,object>();
            routeValues.TryAdd("Area", "");

            return _generator.GenerateActionLink(ViewContext, string.Empty, action, "Blog", null, null, null, routeValues, null);
        }
    }
}