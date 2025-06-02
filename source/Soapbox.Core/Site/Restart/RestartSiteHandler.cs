namespace Soapbox.Application.Site.Restart;

using Alkaline64.Injectable;
using Microsoft.Extensions.Hosting;
using Soapbox.Domain.Results;
using System;

[Injectable]
public class RestartSiteHandler
{
    private readonly IHostApplicationLifetime _appLifetime;

    public RestartSiteHandler(IHostApplicationLifetime appLifetime)
    {
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
    }

    public Result RestartSite()
    {
        _appLifetime.StopApplication();

        return Result.Success();
    }
}
