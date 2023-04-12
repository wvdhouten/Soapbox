namespace Soapbox.DataAccess.FileSystem
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Soapbox.DataAccess.Abstractions;
    using Microsoft.AspNetCore.Identity;

    public static class DependencyInjection
    {
        public static IServiceCollection AddFileSystemStorage([NotNull] this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddScoped<IBlogService, BlogService>();

            return services;
        }

        public static IdentityBuilder AddFileSystemStore(this IdentityBuilder builder)
        {
            builder.AddUserStore<UserStore>();

            return builder;
        }

        public static IApplicationBuilder UseFileSystemStorage(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
            }

            return app;
        }
    }
}
