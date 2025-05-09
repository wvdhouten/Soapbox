namespace Soapbox.Web.Health;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class SoapboxHealthChecks : IHealthCheck
{
    private readonly IWebHostEnvironment _env;

    public SoapboxHealthChecks(IWebHostEnvironment env)
    {
        _env = env;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Content")))
            Task.FromResult(HealthCheckResult.Degraded("Content directory not found"));

        // TODO: Add additional checks.

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
