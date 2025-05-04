namespace Soapbox.Web.Health;

using Microsoft.IdentityModel.Tokens;

public class SiteRepairService
{
    public (bool isOk, IEnumerable<string> errorMessages) RepairSite()
    {
        var errorMessages = new List<string>();

        TryCreateContentDirectories(ref errorMessages);

        return (errorMessages.IsNullOrEmpty(), errorMessages);
    }

    public bool TryCreateContentDirectories(ref List<string> errorMessages)
    {
        var contentPath = Path.Combine(Environment.CurrentDirectory, "Content");
        var contentFolders = new string[] { "Posts", "Pages", "Media" };
        var logPath = Path.Combine(Environment.CurrentDirectory, "Logs");

        try
        {
            Directory.CreateDirectory(contentPath);
            foreach (var folder in contentFolders)
                Directory.CreateDirectory(Path.Combine(contentPath, folder));
            Directory.CreateDirectory(logPath);

            return true;
        }
        catch
        {
            errorMessages.Add("Failed to create content directories.");

            return false;
        }
    }
}
