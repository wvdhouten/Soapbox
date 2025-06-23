namespace Soapbox.Web.Health;

using Alkaline64.Injectable;
using Soapbox.Application.Constants;

[Injectable(Lifetime.Singleton)]
public class SiteRepairService
{
    public (bool isOk, IEnumerable<string> errorMessages) RepairSite()
    {
        var errorMessages = new List<string>();

        TryCreateContentDirectories(ref errorMessages);

        return (errorMessages.Count == 0, errorMessages);
    }

    private static bool TryCreateContentDirectories(ref List<string> errorMessages)
    {
        var contentPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Content);
        var contentFolders = new string[] { FolderNames.Identity, FolderNames.Media, FolderNames.Posts, FolderNames.Pages, FolderNames.Views };
        var logsPath = Path.Combine(Environment.CurrentDirectory, FolderNames.Logs);

        try
        {
            Directory.CreateDirectory(contentPath);
            foreach (var folder in contentFolders)
                Directory.CreateDirectory(Path.Combine(contentPath, folder));
            Directory.CreateDirectory(logsPath);

            return true;
        }
        catch
        {
            errorMessages.Add("Failed to create content directories.");

            return false;
        }
    }
}
