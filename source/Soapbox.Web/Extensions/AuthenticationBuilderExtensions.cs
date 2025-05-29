namespace Soapbox.Web.Extensions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal static class AuthenticationBuilderExtensions
{
    internal static AuthenticationBuilder TryAddGoogle(this AuthenticationBuilder auth, IConfigurationSection section)
    {
        return !section.Exists()
            ? auth
            : auth.AddGoogle(options =>
            {
                options.ClientId = section["ClientId"] ?? string.Empty;
                options.ClientSecret = section["ClientSecret"] ?? string.Empty;
            });
    }

    internal static AuthenticationBuilder TryAddMicrosoft(this AuthenticationBuilder auth, IConfigurationSection section)
    {
        return !section.Exists()
            ? auth
            : auth.AddMicrosoftAccount(options =>
            {
                options.ClientId = section["ClientId"] ?? string.Empty;
                options.ClientSecret = section["ClientSecret"] ?? string.Empty;
            });
    }

    internal static AuthenticationBuilder TryAddFacebook(this AuthenticationBuilder auth, IConfigurationSection section)
    {
        return !section.Exists()
            ? auth
            : auth.AddFacebook(options =>
            {
                options.ClientId = section["ClientId"] ?? string.Empty;
                options.ClientSecret = section["ClientSecret"] ?? string.Empty;
            });
    }

    internal static AuthenticationBuilder TryAddTwitter(this AuthenticationBuilder auth, IConfigurationSection section)
    {
        return !section.Exists()
            ? auth
            : auth.AddTwitter(options =>
            {
                options.ConsumerKey = section["ConsumerKey"];
                options.ConsumerSecret = section["ConsumerSecret"];
            });
    }

    internal static AuthenticationBuilder TryAddGitHub(this AuthenticationBuilder auth, IConfigurationSection section)
    {
        return !section.Exists()
            ? auth
            : auth.AddGitHub(options =>
            {
                options.ClientId = section["ClientId"] ?? string.Empty;
                options.ClientSecret = section["ClientSecret"] ?? string.Empty;
            });
    }

    internal static AuthenticationBuilder TryAddYahoo(this AuthenticationBuilder auth, IConfigurationSection section)
    {
        return !section.Exists()
            ? auth
            : auth.AddYahoo(options =>
            {
                options.ClientId = section["ClientId"] ?? string.Empty;
                options.ClientSecret = section["ClientSecret"] ?? string.Empty;
            });
    }
}
