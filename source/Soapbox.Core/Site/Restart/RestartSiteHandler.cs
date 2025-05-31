namespace Soapbox.Application.Site.Restart;

using Microsoft.Extensions.Hosting;
using System;

public class RestartSiteHandler
{
    private readonly IHostApplicationLifetime _appLifetime;

    public RestartSiteHandler(IHostApplicationLifetime appLifetime)
    {
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
    }

    public void RestartSite()
    {
        _appLifetime.StopApplication();

        throw new ApplicationException("Application restart requested.");
    }
}
