namespace Soapbox.Web.Error.GetErrorDetails;

using System;
using System.Diagnostics;
using Alkaline64.Injectable;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Soapbox.Domain.Results;

[Injectable]
public class GetErrorDetailsHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GetErrorDetailsHandler> _logger;

    public GetErrorDetailsHandler(IHttpContextAccessor httpContextAccessor, ILogger<GetErrorDetailsHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Result<GenericErrorDetails> Handle(int? statusCode)
    {
        return statusCode switch
        {
            404 => GetNotFoundDetails(),
            403 => GetAccessDeniedDetails(),
            _ => GetGenericErrorDetails(),
        };
    }

    private NotFoundErrorDetails GetNotFoundDetails()
    {
        var httpContext = _httpContextAccessor.HttpContext!;
        var feature = httpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var path = feature?.OriginalPath;

        _logger.LogWarning("Page not found: {Path}", path);

        return new NotFoundErrorDetails(Activity.Current?.Id ?? httpContext.TraceIdentifier)
        {
            RequestedUrl = path,
            RedirectUrl = httpContext.Request.GetDisplayUrl()
        };
    }

    private AccessDeniedErrorDetails GetAccessDeniedDetails()
    {
        var httpContext = _httpContextAccessor.HttpContext!;
        var feature = httpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var path = feature?.OriginalPath;

        _logger.LogWarning("Access denied: {Path}", path);

        return new AccessDeniedErrorDetails(Activity.Current?.Id ?? httpContext.TraceIdentifier);
    }

    private GenericErrorDetails GetGenericErrorDetails()
    {
        var httpContext = _httpContextAccessor.HttpContext!;
        var feature = httpContext.Features.Get<IExceptionHandlerPathFeature>()!;

        var model = new GenericErrorDetails(Activity.Current?.Id ?? httpContext.TraceIdentifier);
        if (feature.Error is InvalidOperationException)
            model.Message = feature.Error.Message;

        // TODO: Add custom message for other Exceptions?

        _logger.LogError("Uncaught error occurred: {Error}.", feature.Error.Message);

        return model;
    }
}
