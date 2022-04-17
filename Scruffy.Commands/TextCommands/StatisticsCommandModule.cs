﻿using Discord.Commands;

using Scruffy.Services.Discord;
using Scruffy.Services.Discord.Attributes;
using Scruffy.Services.Statistics;

namespace Scruffy.Commands.TextCommands;

/// <summary>
/// Calendar commands
/// </summary>
[Group("statistics")]
[Alias("stats", "st")]
[RequireDeveloperPermissions]
[BlockedChannelCheck]
public class StatisticsCommandModule : LocatedTextCommandModuleBase
{
    #region Properties

    /// <summary>
    /// Visualizer
    /// </summary>
    public StatisticsVisualizerService VisualizerService { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adding a one time event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [Command("me")]
    [RequireContext(ContextType.Guild)]
    public Task Me() => VisualizerService.PostMeOverview(Context);

    #endregion // Methods
}