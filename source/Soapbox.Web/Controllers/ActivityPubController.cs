namespace Soapbox.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Soapbox.Core.Settings;

    //[Route(".well-known")]
    //public class ActivityPubController : Controller
    //{
    //    private readonly SiteSettings _settings;

    //    public ActivityPubController(IOptionsSnapshot<SiteSettings> settings)
    //    {
    //        _settings = settings.Value;
    //    }

    //    [Produces("application/jrd+json")]
    //    [HttpGet("webfinger")]
    //    public ActionResult WebFinger([FromQuery] string resource)
    //    {
    //        var usersUrl = new Uri(new Uri(_settings.SiteConfiguration.MastodonServerUrl), string.Format("users/{0}", _settings.SiteConfiguration.MastodonAccount.Remove(0, 1))).AbsoluteUri;
    //        var accountUrl = new Uri(new Uri(_settings.SiteConfiguration.MastodonServerUrl), string.Format("{0}", _settings.SiteConfiguration.MastodonAccount)).AbsoluteUri;
    //        var authUrl = new Uri(new Uri(_settings.SiteConfiguration.MastodonServerUrl), "authorize_interaction").AbsoluteUri + "?uri={uri}";

    //        if (_settings.SiteConfiguration.MastodonServerUrl.IsNullOrWhiteSpace() ||
    //            _settings.SiteConfiguration.MastodonAccount.IsNullOrWhiteSpace())
    //        {
    //            return NoContent();
    //        }

    //        var results = new Root
    //        {
    //            subject = string.Format("acct:{0}@{1}", _settings.SiteConfiguration.MastodonAccount.Remove(0, 1), new Uri(_settings.SiteConfiguration.MastodonServerUrl).Host),
    //            aliases = new List<string> { accountUrl, usersUrl },

    //            links = new List<Link>
    //            {
    //                new Link() { rel="http://webfinger.net/rel/profile-page", type="text/html", href=accountUrl },
    //                new Link() { rel="self", type=@"application/activity+json", href=usersUrl},
    //                new Link() { rel="http://ostatus.org/schema/1.0/subscribe", template=authUrl }
    //            }
    //        };

    //        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    //        return Json(results, options);
    //    }
    //}
}
