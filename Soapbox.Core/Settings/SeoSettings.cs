namespace Soapbox.Core.Settings
{
    using System.Linq;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Extensions;
    using Soapbox.Models;

    public class SeoSettings
    {
        private readonly SiteSettings _settings;
        private string _title;

        public string Title
        {
            get
            {
                var title = _title ?? Post?.Title;
                return (title != null)
                    ? $"{title} - {_settings.Title}"
                    : _settings.Title;
            }
            set => _title = value;
        }

        public string Description => Post?.Excerpt?.Clip(100) ?? _settings.Description;

        public string Keywords => Post != null
            ? string.Join(" ,", Post.Categories.Select(c => c.Name))
            : _settings.Keywords;

        public string Author => Post != null
            ? !string.IsNullOrEmpty(Post.Author.DisplayName)
              ? Post.Author.DisplayName
              : Post.Author.UserName
            : _settings.Owner;

        public string Owner => _settings.Owner;

        public string Canonical => Post != null
            ? $"Blog/{Post.Slug}"
            : null;

        public Post Post { get; set; }

        public SeoSettings(IOptionsSnapshot<SiteSettings> options)
        {
            _settings = options.Value;
        }
    }
}
