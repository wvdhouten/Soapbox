namespace Soapbox.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Soapbox.Web.Controllers.Base;

[Authorize]
public partial class AccountController : SoapboxControllerBase
{
}