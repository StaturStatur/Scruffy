using System.Security.Claims;

using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Scruffy.Data.Entity;
using Scruffy.Data.Entity.Repositories.Discord;

namespace Scruffy.ServiceHosts.IdentityServer.Pages.ExternalLogin;

/// <summary>
/// Callback page
/// </summary>
[AllowAnonymous]
public class Callback : PageModel
{
    #region Fields

    /// <summary>
    /// Events
    /// </summary>
    private readonly IEventService _events;

    /// <summary>
    /// Repository factory
    /// </summary>
    private readonly RepositoryFactory _dbFactory;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="events">Events</param>
    /// <param name="dbFactory">Repository factory</param>
    public Callback(IEventService events, RepositoryFactory dbFactory)
    {
        _events = events;
        _dbFactory = dbFactory;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public async Task<IActionResult> OnGet()
    {
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme)
                                      .ConfigureAwait(false);
        if (result.Succeeded == false)
        {
            return RedirectToPage("/Account/AccessDenied");
        }

        var nameIdentifier = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (nameIdentifier == null)
        {
            return RedirectToPage("/Account/AccessDenied");
        }

        if (result.Properties?.Items["scheme"] != "Discord")
        {
            return RedirectToPage("/Account/AccessDenied");
        }

        var discordAccountId = Convert.ToUInt64(nameIdentifier);
        var userId = _dbFactory.GetRepository<DiscordAccountRepository>()
                               .GetQuery()
                               .Where(obj => obj.Id == discordAccountId)
                               .Select(obj => obj.UserId)
                               .FirstOrDefault();

        if (userId == 0)
        {
            return RedirectToPage("/Account/AccessDenied");
        }

        var identityServerUser = new IdentityServerUser(discordAccountId.ToString())
        {
            DisplayName = result.Principal.FindFirst(ClaimTypes.Name)?.Value,
            IdentityProvider = "Discord"
        };

        await HttpContext.SignInAsync(identityServerUser)
                         .ConfigureAwait(false);
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme)
                         .ConfigureAwait(false);

        await _events.RaiseAsync(new UserLoginSuccessEvent("Discord", nameIdentifier, identityServerUser.SubjectId, identityServerUser.DisplayName))
                     .ConfigureAwait(false);

        return Redirect(result.Properties.Items["returnUrl"] ?? "~/");
    }

    #endregion // Methods
}