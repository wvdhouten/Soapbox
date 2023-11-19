namespace Soapbox.Web.Extensions
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Soapbox.Core.Email;
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Core.FileManagement;
    using Soapbox.Core.Markdown;
    using Soapbox.Core.Settings;
    using Soapbox.DataAccess.Sqlite;
    using Soapbox.Models;
    using Soapbox.Web.Helpers;
    using Soapbox.Web.Identity;
    using Soapbox.Web.Services;
    using WilderMinds.MetaWeblog;

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureSoapbox(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddHealthChecks().AddCheck<SoapboxHealthChecks>("soapbox");

            services.AddAutoMapper(typeof(Program));
            services.AddSqlite(configuration, env);

            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IEmailRenderer, RazorEmailRenderer>();

            services.Configure<IdentityOptions>(configuration.GetSection("IdentityOptions"));
            services.AddScoped<AccountService>();
            services.AddScoped<IUserClaimsPrincipalFactory<SoapboxUser>, SoapboxUserClaimsPrincipalFactory>();

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
            .AddSqliteStore();

            services.AddSingleton<SiteRepairService>();

            var siteSettingsSection = configuration.GetSection(nameof(SiteSettings));
            services.Configure<SiteSettings>(siteSettingsSection);
            services.AddScoped<ConfigFileService>();
            services.AddScoped<MediaFileService>();

            services.AddOptions();
            services.AddMemoryCache();

            services.AddLogging();

            services.AddSingleton<IMarkdownParser, MarkdownParser>();

            services.AddMetaWeblog<Services.MetaWeblogService>();

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
}
