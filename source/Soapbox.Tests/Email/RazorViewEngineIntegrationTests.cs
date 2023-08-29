namespace Soapbox.Tests.Email
{
    using System.Threading.Tasks;
    using Soapbox.Core.Email;
    using Xunit;

    public class RazorViewEngineIntegrationTests
    {
        [Fact]
        public async Task Test()
        {
            // Arrange
            var renderer = new RazorEmailRenderer(null, null, null, null);

            // Act

            // Assert
        }
    }
}
