﻿using System.Collections.Concurrent;

using Discord.Commands;

namespace Scruffy.Services.Discord.Attributes;

/// <summary>
/// Developer permissions
/// </summary>
public class RequireDeveloperPermissionsAttribute : PreconditionAttribute
{
    #region Fields

    /// <summary>
    /// User ids
    /// </summary>
    private static ConcurrentDictionary<ulong, byte> _userIds;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    static RequireDeveloperPermissionsAttribute()
    {
        var userIds = Environment.GetEnvironmentVariable("SCRUFFY_DEVELOPER_USER_IDS") ?? string.Empty;

        _userIds = new ConcurrentDictionary<ulong, byte>(userIds.Split(";").ToDictionary(Convert.ToUInt64, obj => (byte)0));
    }

    #endregion // Constructor

    #region PreconditionAttribute

    /// <summary>
    /// Checks if the <paramref name="command" /> has the sufficient permission to be executed.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="command">The command being executed.</param>
    /// <param name="services">The service collection used for dependency injection.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        return Task.FromResult(_userIds.ContainsKey(context.User.Id)
                                   ? PreconditionResult.FromSuccess()
                                   : PreconditionResult.FromError("The channel is blocked for commands."));
    }

    #endregion // PreconditionAttribute
}