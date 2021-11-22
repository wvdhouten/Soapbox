namespace Soapbox.Web.Services
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public class SoapboxHealthChecks : IHealthCheck
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SoapboxHealthChecks(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(Path.Combine(_webHostEnvironment.ContentRootPath, "Content")))
            {
                Task.FromResult(HealthCheckResult.Degraded("Content directory not found"));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
