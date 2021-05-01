﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using Scruffy.Services.Core;

namespace Scruffy.ServiceHost.Discord
{
    /// <summary>
    /// Management of the discord bot
    /// </summary>
    public sealed class DiscordBot : IAsyncDisposable
    {
        #region Fields

        /// <summary>
        /// Client
        /// </summary>
        private DiscordClient _discordClient;

        /// <summary>
        /// Commands
        /// </summary>
        private CommandsNextExtension _commands;

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Start the discord bot
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task StartAsync()
        {
            var config = new DiscordConfiguration
                         {
                             Token = Environment.GetEnvironmentVariable("SCRUFFY_DISCORD_TOKEN"),
                             TokenType = TokenType.Bot,
                             AutoReconnect = true,
                         };

            _discordClient = new DiscordClient(config);
            _discordClient.Ready += OnClientReady;

            _discordClient.UseInteractivity(new InteractivityConfiguration
                                            {
                                                Timeout = TimeSpan.FromMinutes(2)
                                            });

            GlobalServiceProvider.Current.AddSingleton(_discordClient);

            _commands = _discordClient.UseCommandsNext(new CommandsNextConfiguration
                                                       {
                                                           StringPrefixes = new[] { "§" },
                                                           EnableDms = true,
                                                           EnableMentionPrefix = true,
                                                           CaseSensitive = false,
                                                           DmHelp = false,
                                                           Services = GlobalServiceProvider.Current.GetServiceProvider()
                                                       });

            _commands.RegisterCommands(Assembly.Load("Scruffy.Commands"));

            await _discordClient.ConnectAsync();
        }

        /// <summary>
        /// The client entered the ready state
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            await _discordClient.UpdateStatusAsync(new DiscordActivity("you!", ActivityType.Watching));
        }

        #endregion // Methods

        #region IAsyncDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_discordClient != null)
            {
                await _discordClient.DisconnectAsync();

                _discordClient.Dispose();
            }
        }

        #endregion // IAsyncDisposable
    }
}
