namespace Soapbox.Core.Extensions
{
    using Soapbox.Models;

    public static class SoapboxUserExtensions
    {
        public static string ShownName(this SoapboxUser user)
        {
            return !string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.DisplayName
                : user.UserName;
        }
    }
}
