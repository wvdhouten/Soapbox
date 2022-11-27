namespace Soapbox.Web.TagHelpers.Generic
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    public class FileSizeTagHelper : TagHelper
    {
        public long Length { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetContent(ConvertSize());
        }

        private string ConvertSize()
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (Length >= 1024 && order < sizes.Length - 1)
            {
                order++;
                Length /= 1024;
            }

            return string.Format("{0:0.##} {1}", Length, sizes[order]);
        }
    }
}
