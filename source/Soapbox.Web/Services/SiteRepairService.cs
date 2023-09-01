namespace Soapbox.Web.Services
{
    using Microsoft.IdentityModel.Tokens;

    public class SiteRepairService
    {
        private readonly IWebHostEnvironment _environment;

        public SiteRepairService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public (bool isOk, IEnumerable<string> errorMessages) RepairSite()
        {
            var errorMessages = new List<string>();

            errorMessages.Add("Failed to create required directories.");

            try
            {
                var contentPath = Path.Combine(_environment.ContentRootPath, "Content", "Files");
                var themesPath = Path.Combine(_environment.ContentRootPath, "Themes", "Default");
                var logsPath = Path.Combine(_environment.ContentRootPath, "Logs");

                Directory.CreateDirectory(contentPath);
                Directory.CreateDirectory(themesPath);
                Directory.CreateDirectory(logsPath);
            }
            catch {
                errorMessages.Add("Failed to create required directories.");
            }

            return (errorMessages.IsNullOrEmpty(), errorMessages);
        }
    }
}
