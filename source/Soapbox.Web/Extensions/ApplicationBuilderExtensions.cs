namespace Soapbox.Web.Extensions
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;
    using Soapbox.DataAccess.Sqlite;
    using Soapbox.Web.Helpers;
    using Soapbox.Web.Services;
    using WilderMinds.MetaWeblog;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder Configure(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var (isOk, errorMessages) = app.RepairSite();

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

            app.UseHttpsRedirection();
            app.UseStatusCodePagesWithReExecute("/Pages/Error", "?statusCode={0}");

            if (!isOk)
            {
                app.Run(async context => await context.Response.WriteAsync(string.Join(Environment.NewLine, errorMessages)));
            }

            app.UseStaticFiles(env);
            app.UseCookiePolicy();

            app.UseMetaWeblog("/livewriter");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/Health");

                endpoints.MapControllerRoute("area", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Pages}/{action=Index}/{id?}");
            });

            return app;
        }

        private static (bool isOk, IEnumerable<string> errorMessages) RepairSite(this IApplicationBuilder app)
        {
            return app.ApplicationServices.GetRequiredService<SiteRepairService>().RepairSite();
        }

        private static void UseStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = CacheControlPrepareResponse });

            var mediaFileProvider = new PhysicalFileProvider(new DirectoryInfo(Path.Combine(env.ContentRootPath, "Content", "Files")).FullName);
            var mediaFileOptions = new StaticFileOptions { FileProvider = mediaFileProvider, RequestPath = "/Media", OnPrepareResponse = CacheControlPrepareResponse };
            app.UseStaticFiles(mediaFileOptions);

            var themeFileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Themes"));
            var themeFileOptions = new StaticFileOptions { FileProvider = themeFileProvider, RequestPath = "/Themes", OnPrepareResponse = CacheControlPrepareResponse };
            app.UseStaticFiles(themeFileOptions);

            env.WebRootFileProvider = new ExtendedCompositeFileProvider(env.WebRootFileProvider, mediaFileOptions, themeFileOptions);
        }

        private static void CacheControlPrepareResponse(StaticFileResponseContext context)
        {
            context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={TimeSpan.FromDays(365).TotalSeconds},immutable";
        }
    }
}
