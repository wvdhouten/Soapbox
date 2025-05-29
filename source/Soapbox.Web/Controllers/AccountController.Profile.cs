namespace Soapbox.Web.Controllers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Soapbox.Domain.Results;
using Soapbox.Identity.Profile;
using Soapbox.Identity.Profile.GetPersonalData;
using Soapbox.Identity.Profile.GetProfile;
using Soapbox.Identity.Profile.UpdateProfile;

public partial class AccountController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromServices] GetProfileHandler query)
    {
        var result = await query.GetProfileAsync(User);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => View(result.Value)
        };
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromServices] UpdateProfileHandler command, [FromForm] UserProfile model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await command.UpdateProfileAsync(model);
        return result.IsSuccess switch
        {
            false when result.Error?.Code == ErrorCode.NotFound => NotFound(result.Error.Message),
            false => BadRequest("Unable to process request."),
            _ => WithStatusMessage(result.Value).RedirectToAction(nameof(Index)),
        };
    }

    [HttpPost]
    public async Task<IActionResult> DownloadPersonalData([FromServices] GetPersonalDataHandler handler)
    {
        var result = await handler.GetPersonalDataAsync(User);
        return result.IsSuccess switch
        {
            false => BadRequest("Unable to process request."),
            _ => File(JsonSerializer.SerializeToUtf8Bytes(result.Value, JsonSerializerOptions.Default), "application/json", "PersonalData.json"),
        };
    }
}
