namespace Soapbox.Web.Extensions;

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Soapbox.DataAccess.FileSystem.Extensions;
using Soapbox.Web.Health;
using Soapbox.Web.Helpers;
using WilderMinds.MetaWeblog;

public static class WebApplicationExtensions
{
    public static WebApplication Configure(this WebApplication app, IWebHostEnvironment env)
    {
        var (isOk, errorMessages) = app.RepairSite();

        app.UseFileSystemStorageAsync();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        if (!isOk)
        {
            app.Run(async context => await context.Response.WriteAsync(string.Join(Environment.NewLine, errorMessages)));
        }

        app.UseStaticFiles(env);
        app.UseCookiePolicy();

        app.UseMetaWeblog("/livewriter");
        app.UseResponseCaching();
        app.UseRouting();
        app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/Health");

        app.MapAreaControllerRoute("admin", "Admin", "Admin/{controller=Home}/{action=Index}/{id?}");
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        return app;
    }

    private static (bool isOk, IEnumerable<string> errorMessages) RepairSite(this IApplicationBuilder app)
    {
        return app.ApplicationServices.GetRequiredService<SiteRepairService>().RepairSite();
    }

    private static void UseStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = CacheControlPrepareResponse });

        var mediaFileProvider = new PhysicalFileProvider(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Content", "Files")).FullName);
        var mediaFileOptions = new StaticFileOptions { FileProvider = mediaFileProvider, RequestPath = "/Media", OnPrepareResponse = CacheControlPrepareResponse };
        app.UseStaticFiles(mediaFileOptions);

        var themeFileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "Themes"));
        var themeFileOptions = new StaticFileOptions { FileProvider = themeFileProvider, RequestPath = "/Themes", OnPrepareResponse = CacheControlPrepareResponse };
        app.UseStaticFiles(themeFileOptions);

        env.WebRootFileProvider = new ExtendedCompositeFileProvider(env.WebRootFileProvider, mediaFileOptions, themeFileOptions);
    }

    private static void CacheControlPrepareResponse(StaticFileResponseContext context)
    {
        context.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={TimeSpan.FromDays(365).TotalSeconds},immutable";
    }
}
