namespace Soapbox.Models
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Identity;

    public class SoapboxUser : IdentityUser
    {
        [PersonalData]
        public string DisplayName { get; set; }

        public UserRole Role { get; set; }

        public IList<Post> Posts { get; set; }
    }
}
