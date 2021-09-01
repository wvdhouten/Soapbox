namespace Soapbox.DataAccess.Sqlite
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Soapbox.Core.Identity;
    using Soapbox.DataAccess.Data;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.DataAccess.Sqlite.Repositories;

    public static class DependencyInjection
    {
        public static IServiceCollection AddSqlite([NotNull] this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetSection("SqlLite").GetValue<string>("ConnectionString")
            ));

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDefaultIdentity<SoapboxUser>().AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<IPostRepository, PostRepository>();

            return services;
        }

        public static IApplicationBuilder UseSqlite(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }

            return app;
        }
    }
}
