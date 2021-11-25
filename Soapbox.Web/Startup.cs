namespace Soapbox.Web
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;
    using Soapbox.Core.Email;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks().AddCheck<SoapboxHealthChecks>("soapbox");

            services.AddAutoMapper(typeof(Startup));
            services.AddSqlite(Configuration, HostEnvironment);

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

            //services.AddSingleton<IAuthorizationHandler, OwnerAuthorizationHandler>();
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("EditPostPolicy", policy => policy.Requirements.Add(new OwnerRequirement()));
            //});

            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));
            services.AddScoped<ConfigFileService>();

            services.AddSingleton<IMarkdownParser, MarkdownParser>();

            services.AddMetaWeblog<Services.MetaWeblogService>();

            services.AddHttpContextAccessor();
            services.AddControllersWithViews();

            services.AddTransient<ViewLocationExpander>();
            services.AddOptions<RazorViewEngineOptions>().Configure<ViewLocationExpander>((options, expander) =>
            {
                options.ViewLocationExpanders.Add(expander);
            });

            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddScssBundle("/css/bundle.css", "scss/site.scss");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSqlite(env);

            if (!env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Pages/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Pages/Error", "?statusCode={0}");
            app.UseWebOptimizer();

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            static void CacheControlPrepareResponse(StaticFileResponseContext context)
            {
                context.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + 60 * 60 * 24;
                context.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddHours(12).ToString("R");
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(new DirectoryInfo(Path.Combine(env.ContentRootPath, "Content", "Files")).FullName),
                RequestPath = "/Content/Files",
                OnPrepareResponse = CacheControlPrepareResponse
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Themes")),
                RequestPath = "/Themes",
                OnPrepareResponse = CacheControlPrepareResponse
            });

            app.UseMetaWeblog("/livewriter");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // TODO: Add NotFound
                endpoints.MapHealthChecks("/Health");

                endpoints.MapControllerRoute(name: "area", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Pages}/{action=Index}/{id?}");
            });
        }
    }
}
