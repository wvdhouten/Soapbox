namespace Soapbox.Web.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Soapbox.Application.Email;
using Soapbox.Application.Email.Abstractions;
using Soapbox.Application.FileManagement;
using Soapbox.Application.Markdown;
using Soapbox.Application.Settings;
using Soapbox.Domain.Users;
using Soapbox.Web.Helpers;
using Soapbox.Web.Identity;
using WilderMinds.MetaWeblog;
using Alkaline64.Injectable.Extensions;
using Soapbox.DataAccess.FileSystem.Extensions;
using Soapbox.Web.Email;
using Soapbox.Web.Health;
using Soapbox.Domain.Markdown;

public static class ServiceCollectionExtensions
{
    public static void ConfigureSoapbox(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddHealthChecks().AddCheck<SoapboxHealthChecks>("soapbox");

        // services.AddSqlite(configuration, env);
        services.AddFileSystemStorage();

        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailRenderer, RazorEmailRenderer>();

        services.Configure<IdentityOptions>(configuration.GetSection("IdentityOptions"));
        services.AddScoped<AccountService>();
        services.AddScoped<IUserClaimsPrincipalFactory<SoapboxUser>, SoapboxUserClaimsPrincipalFactory>();

        services.RegisterInjectables<Program>();

        var auth = services.AddAuthentication(o =>
        {
            o.DefaultScheme = IdentityConstants.ApplicationScheme;
            o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .TryAddGoogle(configuration.GetSection("Authentication:Google"))
        .TryAddMicrosoft(configuration.GetSection("Authentication:Microsoft"))
        .TryAddFacebook(configuration.GetSection("Authentication:Facebook"))
        .TryAddTwitter(configuration.GetSection("Authentication:Twitter"))
        .TryAddGitHub(configuration.GetSection("Authentication:GitHub"))
        .TryAddYahoo(configuration.GetSection("Authentication:Yahoo"))
        .AddIdentityCookies();

        services.AddIdentityCore<SoapboxUser>(o =>
        {
            o.Stores.MaxLengthForKeys = 128;
        })
        .AddDefaultTokenProviders()
        .AddSignInManager()
        .AddFileSystemStore();
        //.AddSqliteStore();

        services.AddSingleton<SiteRepairService>();

        var siteSettingsSection = configuration.GetSection(nameof(SiteSettings));
        services.Configure<SiteSettings>(siteSettingsSection);
        services.AddScoped<ConfigFileService>();
        services.AddScoped<MediaFileService>();

        services.AddOptions();
        services.AddMemoryCache();

        services.AddLogging();

        services.AddSingleton<IMarkdownParser, MarkdownParser>();

        services.AddMetaWeblog<MetaWeblog.MetaWeblogService>();

        services.AddHttpContextAccessor();
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = false;
        });
        services.AddControllersWithViews();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => siteSettingsSection.GetValue<bool>(nameof(SiteSettings.CookieConsentEnabled));
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddSingleton<ViewLocationExpander>();
        services.AddOptions<RazorViewEngineOptions>().Configure<ViewLocationExpander>((options, expander) =>
        {
            options.ViewLocationExpanders.Add(expander);
        });

        if (env.IsDevelopment())
        {
            services.AddSassCompiler();
        }
    }
}
