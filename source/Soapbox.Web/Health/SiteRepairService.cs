namespace Soapbox.Web.Health;

public class SiteRepairService
{
    public (bool isOk, IEnumerable<string> errorMessages) RepairSite()
    {
        var errorMessages = new List<string>();

        TryCreateContentDirectories(ref errorMessages);

        return (errorMessages.Count == 0, errorMessages);
    }

    private bool TryCreateContentDirectories(ref List<string> errorMessages)
    {
        var contentPath = Path.Combine(Environment.CurrentDirectory, "Content");
        var contentFolders = new string[] { "Indentity", "Posts", "Pages", "Media" };
        var logsPath = Path.Combine(Environment.CurrentDirectory, "Logs");

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
