namespace Soapbox.Web.Health;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Soapbox.Application.Constants;

public class FileSystemChecks : IHealthCheck
{
    private readonly IWebHostEnvironment _env;

    public FileSystemChecks(IWebHostEnvironment env)
    {
        _env = env;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, FolderNames.Content)))
            Task.FromResult(HealthCheckResult.Degraded("Content directory not found"));

        // TODO: Add additional checks.

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
