namespace Soapbox.DataAccess.FileSystem.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Alkaline64.Injectable.Extensions;
using Soapbox.Domain.Users;
using Soapbox.DataAccess.Abstractions;
using Soapbox.DataAccess.FileSystem.Abstractions;

public static class DependencyInjection
{
    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services)
    {
        services.RegisterInjectables<UserFileSystemStore>();

        return services;
    }

    public static IdentityBuilder AddFileSystemStore(this IdentityBuilder builder)
    {
        builder.AddUserStore<UserFileSystemStore>();
        // builder.AddRoleStore<RoleStore>();

        return builder;
    }

    public static IApplicationBuilder UseFileSystemStorage(this IApplicationBuilder app)
    {
        new Task(async () => await app.ApplicationServices
            .CreateScope().ServiceProvider
            .GetRequiredService<ITransactionalUserStore<SoapboxUser>>()
            .InitAsync()).RunSynchronously();

        app.ApplicationServices
            .CreateScope().ServiceProvider
            .GetRequiredService<IBlogStore>()
            .Init();

        return app;
    }
}
