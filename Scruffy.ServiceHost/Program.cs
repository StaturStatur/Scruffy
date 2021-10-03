﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using Scruffy.ServiceHost.Discord;
using Scruffy.Services.Core;
using Scruffy.Services.Core.JobScheduler;
using Scruffy.Services.Core.Localization;
using Scruffy.Services.Fractals;

namespace Scruffy.ServiceHost
{
    /// <summary>
    /// Main class
    /// </summary>
    public class Program
    {
        #region Fields

        /// <summary>
        /// Wait for program exit
        /// </summary>
        private static TaskCompletionSource<bool> _waitForExitTaskSource = new TaskCompletionSource<bool>();

        #endregion // Fields

        #region Methods

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <returns>Task</returns>
        public static async Task Main()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            await using (var jobScheduler = new JobScheduler())
            {
                // TODO configuration
                await using (var stream = Assembly.Load("Scruffy.Data").GetManifestResourceStream("Scruffy.Data.Resources.Languages.de-DE.json"))
                {
                    var localizationService = new LocalizationService();

                    localizationService.Load(stream);

                    GlobalServiceProvider.Current.AddSingleton(localizationService);
                }

                GlobalServiceProvider.Current.AddSingleton(jobScheduler);

                using (var fractalReminderService = new FractalLfgReminderService(jobScheduler))
                {
                    GlobalServiceProvider.Current.AddSingleton(fractalReminderService);

                    await using (var discordBot = new DiscordBot())
                    {
                        await discordBot.StartAsync().ConfigureAwait(false);

                        await jobScheduler.StartAsync().ConfigureAwait(false);

                        await _waitForExitTaskSource.Task.ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// The cancel key was pressed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e) => _waitForExitTaskSource.SetResult(true);

        /// <summary>
        /// Occurs when the default application domain's parent process exits.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Argument</param>
        private static void OnProcessExit(object sender, EventArgs e) => _waitForExitTaskSource.SetResult(true);

        #endregion // Methods
    }
}
