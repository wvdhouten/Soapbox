namespace Soapbox.DataAccess.Sqlite.Extensions;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Soapbox.DataAccess.Abstractions;
using Soapbox.DataAccess.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlite([NotNull] this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddScoped<IBlogRepository, BlogService>();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(configuration.GetSection("SqlLite").GetValue<string>("ConnectionString").Replace("{ContentPath}", $"{env.ContentRootPath}\\Content"));
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
            app.UseMigrationsEndPoint();

        return app;
    }
}
