namespace Soapbox.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;

    public class ExtendedCompositeFileProvider : IFileProvider
    {
        private readonly IFileProvider _webRootFileProvider;
        private readonly IEnumerable<StaticFileOptions> _options;

        public ExtendedCompositeFileProvider(IFileProvider webRootFileProvider, params StaticFileOptions[] staticFileOptions)
            : this(webRootFileProvider, (IEnumerable<StaticFileOptions>)staticFileOptions)
        {
        }

        public ExtendedCompositeFileProvider(IFileProvider webRootFileProvider, IEnumerable<StaticFileOptions> staticFileOptions)
        {
            _webRootFileProvider = webRootFileProvider ?? throw new ArgumentNullException(nameof(webRootFileProvider));
            _options = staticFileOptions;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return GetFileProvider(subpath, out var outpath).GetDirectoryContents(outpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return GetFileProvider(subpath, out var outpath).GetFileInfo(outpath);
        }

        public IChangeToken Watch(string filter)
        {
            return GetFileProvider(filter, out var outpath).Watch(outpath);
        }

        internal IFileProvider GetFileProvider(string path, out string outpath)
        {
            outpath = path;

            if (_options != null)
            {
                foreach (var option in _options)
                {
                    if (path.StartsWith(option.RequestPath, StringComparison.OrdinalIgnoreCase))
                    {
                        outpath = path[option.RequestPath.Value.Length..];

                        return option.FileProvider;
                    }
                }
            }

            return _webRootFileProvider;
        }
    }
}