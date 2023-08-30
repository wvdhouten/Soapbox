namespace Soapbox.Tests.Email
{
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Soapbox.Core.Email.Abstractions;
    using Soapbox.Web.Models.Email;

    public class RazorEmailRendererIntegrationTests
    {
        // This is an end-to-end integration test (incl. the ServiceProvider from the Web project). The email renderer would not let itself be easily tested as a unit.
        [Fact]
        public async Task RenderWelcomeEmail()
        {
            // Arrange
            var server = new WebApplicationFactory<Program>();
            var emailRenderer = server.Services.CreateScope().ServiceProvider.GetService<IEmailRenderer>();

            // Act
            var result = await emailRenderer.Render(nameof(ResetPassword), new ResetPassword { CallbackUrl= "--Reset-Password--" });

            // Assert
            Assert.NotNull(result);
            Assert.Contains("--Reset-Password--", result);
        }
    }
}
