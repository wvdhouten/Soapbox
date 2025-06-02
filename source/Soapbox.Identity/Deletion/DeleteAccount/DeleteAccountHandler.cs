namespace Soapbox.Identity.Deletion.DeleteAccount;

using Alkaline64.Injectable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Soapbox.Domain.Results;
using Soapbox.Domain.Users;
using Soapbox.Identity.Custom;
using System.Threading.Tasks;

[Injectable]
public class DeleteAccountHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly TransactionalUserManager<SoapboxUser> _userManager;
    private readonly SignInManager<SoapboxUser> _signInManager;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(
        IHttpContextAccessor contextAccessor,
        TransactionalUserManager<SoapboxUser> userManager,
        SignInManager<SoapboxUser> signInManager,
        ILogger<DeleteAccountHandler> logger)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<Result> DeleteAccount(DeleteAccountRequest request)
    {
        var userResult = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (userResult.IsFailure)
            return userResult.Error!;

        request.RequirePassword = await _userManager.HasPasswordAsync(userResult);
        if (request.RequirePassword && !await _userManager.CheckPasswordAsync(userResult, request.Password))
            return Error.ValidationError("Account deletion failed.", new() { { nameof(request.Password), "Incorrect password." } } );

        var userId = await _userManager.GetUserIdAsync(userResult);
        var result = await _userManager.DeleteAsync(userResult);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

        return Result.Success();
    }
}
