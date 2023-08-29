namespace Soapbox.Web
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;
    using Soapbox.Core.Email;
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Core.FileManagement;
    using Soapbox.Core.Markdown;
    using Soapbox.Core.Settings;
    using Soapbox.DataAccess.Sqlite;
    using Soapbox.Models;
    using Soapbox.Web.Extensions;
    using Soapbox.Web.Helpers;
    using Soapbox.Web.Identity;
    using Soapbox.Web.Services;
    using WilderMinds.MetaWeblog;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public IWebHostEnvironment HostEnvironment { get; }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<SoapboxHealthChecks>("soapbox");

            services.AddAutoMapper(typeof(Startup));
            services.AddSqlite(Configuration, HostEnvironment);

            services.AddScoped<IEmailRenderer, RazorEmailRenderer>();

            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailClient, SmtpEmailClient>();

            services.Configure<IdentityOptions>(Configuration.GetSection("IdentityOptions"));
            services.AddScoped<AccountService>();
            services.AddScoped<IUserClaimsPrincipalFactory<SoapboxUser>, SoapboxUserClaimsPrincipalFactory>();

            var auth = services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .TryAddGoogle(Configuration.GetSection("Authentication:Google"))
            .TryAddMicrosoft(Configuration.GetSection("Authentication:Microsoft"))
            .TryAddFacebook(Configuration.GetSection("Authentication:Facebook"))
            .TryAddTwitter(Configuration.GetSection("Authentication:Twitter"))
            .TryAddGitHub(Configuration.GetSection("Authentication:GitHub"))
            .TryAddYahoo(Configuration.GetSection("Authentication:Yahoo"))
            .AddIdentityCookies();

            services.AddIdentityCore<SoapboxUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
            })
            .AddDefaultTokenProviders()
            .AddSignInManager()
            .AddSqliteStore();

            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));
            services.AddScoped<ConfigFileService>();
            services.AddScoped<MediaFileService>();

            services.AddSingleton<IMarkdownParser, MarkdownParser>();

            services.AddMetaWeblog<Services.MetaWeblogService>();

            services.AddHttpContextAccessor();
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
            });
            services.AddControllersWithViews();

            services.AddTransient<ViewLocationExpander>();
            services.AddOptions<RazorViewEngineOptions>().Configure<ViewLocationExpander>((options, expander) =>
            {
                options.ViewLocationExpanders.Add(expander);
            });

            if (HostEnvironment.IsDevelopment())
            {
                services.AddSassCompiler();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            RepairSite(env);

            app.UseSqlite(env);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Pages/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Pages/Error", "?statusCode={0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = CacheControlPrepareResponse });

            var mediaFileProvider = new PhysicalFileProvider(new DirectoryInfo(Path.Combine(env.ContentRootPath, "Content", "Files")).FullName);
            var mediaFileOptions = new StaticFileOptions { FileProvider = mediaFileProvider, RequestPath = "/Media", OnPrepareResponse = CacheControlPrepareResponse };
            app.UseStaticFiles(mediaFileOptions);

            var themeFileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Themes"));
            var themeFileOptions = new StaticFileOptions { FileProvider = themeFileProvider, RequestPath = "/Themes", OnPrepareResponse = CacheControlPrepareResponse };
            app.UseStaticFiles(themeFileOptions);

            env.WebRootFileProvider = new ExtendedCompositeFileProvider(env.WebRootFileProvider, mediaFileOptions, themeFileOptions);

            app.UseMetaWeblog("/livewriter");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/Health");

                endpoints.MapControllerRoute(name: "area", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Pages}/{action=Index}/{id?}");
            });
        }

        private static void CacheControlPrepareResponse(StaticFileResponseContext context)
        {
            context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={TimeSpan.FromDays(365).TotalSeconds},immutable";
        }

        private static void RepairSite(IWebHostEnvironment env)
        {
            var contentPath = Path.Combine(env.ContentRootPath, "Content", "Files");
            var themesPath = Path.Combine(env.ContentRootPath, "Themes", "Default");
            var logsPath = Path.Combine(env.ContentRootPath, "Logs");

            EnsureDirectory(contentPath);
            EnsureDirectory(themesPath);
            EnsureDirectory(logsPath);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
