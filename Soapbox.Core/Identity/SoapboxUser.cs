namespace Soapbox.Core.Identity
{
    using Microsoft.AspNetCore.Identity;

    public class SoapboxUser : IdentityUser
    {
        [PersonalData]
        public string DisplayName { get; set; }

        public UserRole Role { get; set; }
    }
}
