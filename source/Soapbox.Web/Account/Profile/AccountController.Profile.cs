namespace Soapbox.Web.Account;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Web.Account.Profile.GetPersonalData;
using Soapbox.Web.Account.Profile.GetProfile;
using Soapbox.Web.Account.Profile.UpdateProfile;
using Soapbox.Web.Common.Base;
using Soapbox.Web.Models.Account;

public partial class AccountController : SoapboxControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromServices] GetProfileQuery query)
    {
        var result = await query.HandleAsync(User);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(result.Value)
        };
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromServices] UpdateProfileCommand command, [FromForm] ProfileModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await command.HandleAsync(model);
        switch (result.IsSuccess)
        {
            case false when result.Error?.Code == ErrorCode.NotFound:
                return NotFound(result.Error.Message);
            case false:
                return BadRequest("Unable to process request.");
            default:
                StatusMessage = result.Value;
                return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DownloadPersonalData([FromServices] GetPersonalDataQuery query)
    {
        var result = await query.HandleAsync(User);
        return result.IsSuccess switch
        {
            false => BadRequest("Unable to process request."),
            _ => File(JsonSerializer.SerializeToUtf8Bytes(result.Value, JsonSerializerOptions.Default), "application/json", "PersonalData.json"),
        };
    }
}
