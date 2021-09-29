namespace Soapbox.DataAccess.Sqlite
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Soapbox.DataAccess.Data;
    using Soapbox.DataAccess.Abstractions;
    using Soapbox.DataAccess.Sqlite.Repositories;
    using Microsoft.AspNetCore.Identity;
    using System.IO;

    public static class DependencyInjection
    {
        public static IServiceCollection AddSqlite([NotNull] this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                Directory.CreateDirectory(env.ContentRootPath);
                options.UseSqlite(configuration.GetSection("SqlLite").GetValue<string>("ConnectionString").Replace("{BasePath}", env.ContentRootPath));
            });
            services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }

        public static IdentityBuilder AddSqliteStore(this IdentityBuilder builder)
        {
            return builder.AddEntityFrameworkStores<ApplicationDbContext>();
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
