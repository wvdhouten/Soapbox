
namespace Soapbox.DataAccess.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Soapbox.Core.Identity;
    using Soapbox.Models;

    public class ApplicationDbContext : IdentityDbContext<SoapboxUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Post> Posts { get; set; }
    }
}
