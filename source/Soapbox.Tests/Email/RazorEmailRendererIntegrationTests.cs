namespace Soapbox.Tests.Email
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Soapbox.Core.Email;
    using Xunit;
    using Soapbox.Models;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Soapbox.Web;
    using Microsoft.Extensions.DependencyInjection;
    using Soapbox.Core.Email.Abstractions;

    public class RazorEmailRendererIntegrationTests
    {
        // This is an end-to-end integration test (incl. the ServiceProvider from the Web project). The email renderer would not let itself be easily tested as a unit.
        [Fact]
        public async Task RenderWelcomeEmail()
        {
            // Arrange
            var server = new WebApplicationFactory<Startup>();
            var emailRenderer = server.Services.CreateScope().ServiceProvider.GetService<IEmailRenderer>();

            // Act
            var result = await emailRenderer.Render("Welcome", "Test");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Welcome Test", result);
        }
    }
}
