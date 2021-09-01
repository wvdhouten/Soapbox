namespace Soapbox.Web.Identity
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Identity;

    public class SoapboxUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<SoapboxUser>
    {
        public SoapboxUserClaimsPrincipalFactory(UserManager<SoapboxUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        { }

        public async override Task<ClaimsPrincipal> CreateAsync(SoapboxUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;
            var claim = new Claim(JwtClaimTypes.Role, user.Role.ToString());
            identity.AddClaim(claim);

            return principal;
        }
    }
}