namespace Soapbox.Web
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Soapbox.Core.Email;
    using Soapbox.Core.Identity;
    using Soapbox.Core.Markdown;
    using Soapbox.DataAccess.Sqlite;
    using Soapbox.Domain.Blog;
    using Soapbox.Web.Identity;
    using Soapbox.Web.Identity.Policies;
    using Soapbox.Web.Services;
    using Soapbox.Web.Settings;

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
            services.AddHttpContextAccessor();
            services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailClient, SmtpEmailClient>();

            services.Configure<IdentityOptions>(Configuration.GetSection("IdentityOptions"));
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserClaimsPrincipalFactory<SoapboxUser>, SoapboxUserClaimsPrincipalFactory>();

            services.AddSqlite(Configuration, HostEnvironment);

            var auth = services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });
            TryAddExternalIdentityProviders(auth);

            services.AddIdentityCore<SoapboxUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
            }).AddDefaultTokenProviders()
            .AddSignInManager()
            .AddSqliteStore();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditPostPolicy", policy => policy.Requirements.Add(new OwnerRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, OwnerAuthorizationHandler>();
            services.AddSingleton<IMarkdownParser, MarkdownParser>();

            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));
            services.AddScoped<IBlogService, BlogService>();

            services.AddRazorPages();
            services.AddWebOptimizer(options =>
            {
                options.AddScssBundle("/css/bundle.css", "scss/site.scss");
                options.AddScssBundle("/css/prism-theme.css", "scss/prism-theme.scss");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebOptimizer();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Pages/Error");
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/Pages/Error");

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

        private void TryAddExternalIdentityProviders(AuthenticationBuilder auth)
        {
            // TODO: Put all identity code in a single module.
            var section = Configuration.GetSection("Authentication:Google");
            if (section.Exists())
            {
                auth.AddGoogle(options =>
                {
                    options.ClientId = section["ClientId"];
                    options.ClientSecret = section["ClientSecret"];
                });
            }

            var msSection = Configuration.GetSection("Authentication:Microsoft");
            if (msSection.Exists())
            {
                auth.AddMicrosoftAccount(options =>
                {
                    options.ClientId = section["ClientId"];
                    options.ClientSecret = section["ClientSecret"];
                });
            }

            var fbSection = Configuration.GetSection("Authentication:Facebook");
            if (fbSection.Exists())
            {
                auth.AddFacebook(options =>
                {
                    options.ClientId = fbSection["ClientId"];
                    options.ClientSecret = fbSection["ClientSecret"];
                });
            }

            var tSection = Configuration.GetSection("Authentication:Twitter");
            if (tSection.Exists())
            {
                auth.AddTwitter(options =>
                {

                    options.ConsumerKey = tSection["ConsumerKey"];
                    options.ConsumerSecret = tSection["ConsumerSecret"];
                });
            }

            var ghSection = Configuration.GetSection("Authentication:GitHub");
            if (ghSection.Exists())
            {
                auth.AddGitHub(options =>
                {
                    options.ClientId = ghSection["ClientId"];
                    options.ClientSecret = ghSection["ClientSecret"];
                });
            }

            auth.AddIdentityCookies();
        }
    }
}
