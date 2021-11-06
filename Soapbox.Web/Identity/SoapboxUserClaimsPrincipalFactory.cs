namespace Soapbox.Web.Identity
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Soapbox.Models;

    /// <summary>
    /// Provides methods to create a claims principal for a given soapbox user.
    /// </summary>
    /// <seealso cref="UserClaimsPrincipalFactory&lt;SoapboxUser&gt;" />
    public class SoapboxUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<SoapboxUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoapboxUserClaimsPrincipalFactory"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="T:Microsoft.AspNetCore.Identity.UserManager`1" /> to retrieve user information from.</param>
        /// <param name="optionsAccessor">The configured <see cref="T:Microsoft.AspNetCore.Identity.IdentityOptions" />.</param>
        public SoapboxUserClaimsPrincipalFactory(UserManager<SoapboxUser> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        { }

        /// <summary>
        /// Creates a <see cref="T:System.Security.Claims.ClaimsPrincipal" /> from an user asynchronously.
        /// </summary>
        /// <param name="user">The user to create a <see cref="T:System.Security.Claims.ClaimsPrincipal" /> from.</param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous creation operation, containing the created <see cref="T:System.Security.Claims.ClaimsPrincipal" />.
        /// </returns>
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