namespace Soapbox.Web.Extensions;

using Microsoft.AspNetCore.Http;

public static class HttpRequestExtensions
{
    public static string BaseUrl(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}";
    }

    public static string FullUrl(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}";
    }
}
