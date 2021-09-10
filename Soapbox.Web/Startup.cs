namespace Soapbox.Web
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Soapbox.Core.Email;
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Core.Identity;
    using Soapbox.DataAccess.Sqlite;
    using Soapbox.Domain;
    using Soapbox.Domain.Abstractions;
    using Soapbox.Web.Config;
    using Soapbox.Web.Identity;
    using Soapbox.Web.Identity.Policies;

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
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailClient, SmtpEmailClient>();

            services.Configure<IdentityOptions>(Configuration.GetSection("IdentityOptions"));
            services.AddScoped<IUserClaimsPrincipalFactory<SoapboxUser>, SoapboxUserClaimsPrincipalFactory>();

            services.AddSqlite(Configuration, HostEnvironment);

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGoogle(options =>
            {
                var section = Configuration.GetSection("Authentication:Google");
                options.ClientId = section["ClientId"];
                options.ClientSecret = section["ClientSecret"];
            })
            .AddMicrosoftAccount(options =>
            {
                var section = Configuration.GetSection("Authentication:Google");
                options.ClientId = section["ClientId"];
                options.ClientSecret = section["ClientSecret"];
            })
            .AddFacebook(options =>
            {
                var section = Configuration.GetSection("Authentication:Google");
                options.ClientId = section["ClientId"];
                options.ClientSecret = section["ClientSecret"];
            })
            .AddTwitter(options =>
            {
                var section = Configuration.GetSection("Authentication:Google");
                options.ConsumerKey = section["ConsumerKey"] ?? "aaa";
                options.ConsumerSecret = section["ConsumerSecret"] ?? "aaa";
            })
            .AddGitHub(options =>
            {
                var section = Configuration.GetSection("Authentication:Google");
                options.ClientId = section["ClientId"];
                options.ClientSecret = section["ClientSecret"];
            }).AddIdentityCookies();

            services.AddIdentityCore<SoapboxUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
            }).AddSignInManager().AddSqliteStore();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditPostPolicy", policy => policy.Requirements.Add(new OwnerRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, DocumentAuthorizationHandler>();

            services.AddScoped<IMarkdownParser, MarkdownParser>();

            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));
            services.AddScoped<IBlogService, BlogService>();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Pages/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSqlite(env);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // TODO: Add NotFound
                // TODO: Add HealthCheck

                endpoints.MapControllerRoute(name: "area", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Pages}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
