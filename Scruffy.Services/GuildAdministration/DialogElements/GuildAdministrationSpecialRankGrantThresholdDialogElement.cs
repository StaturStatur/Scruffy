﻿using System.Globalization;

using DSharpPlus.Entities;

using Scruffy.Services.Core.Discord;
using Scruffy.Services.Core.Localization;

namespace Scruffy.Services.GuildAdministration.DialogElements
{
    /// <summary>
    /// Acquisition of the special rank grant threshold
    /// </summary>
    public class GuildAdministrationSpecialRankGrantThresholdDialogElement : DialogMessageElementBase<double>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        public GuildAdministrationSpecialRankGrantThresholdDialogElement(LocalizationService localizationService)
            : base(localizationService)
        {
        }

        #endregion // Constructor

        #region DialogMessageElementBase<string>

        /// <summary>
        /// Return the message of element
        /// </summary>
        /// <returns>Message</returns>
        public override string GetMessage() => LocalizationGroup.GetText("Message", "Please enter the grant threshold which should be used.");

        /// <summary>
        /// Converting the response message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        public override double ConvertMessage(DiscordMessage message)
        {
            return double.Parse(message.Content, NumberStyles.Any, LocalizationGroup.CultureInfo);
        }

        #endregion // DialogMessageElementBase<string>
    }
}