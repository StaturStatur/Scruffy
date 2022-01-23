﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Scruffy.Data.Entity.Tables.Guild;

namespace Scruffy.Data.Entity.Tables.GuildWars2.Guild;

/// <summary>
/// Members of a Guild Wars 2 guild
/// </summary>
[Table("GuildWarsGuildMembers")]
public class GuildWarsGuildMemberEntity
{
    #region Properties

    /// <summary>
    /// Id of the guild
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Account name
    /// </summary>
    [StringLength(42)]
    public string Name { get; set; }

    /// <summary>
    /// Current rank
    /// </summary>
    public string Rank { get; set; }

    #region Navigation properties

    /// <summary>
    /// Guild
    /// </summary>
    [ForeignKey(nameof(GuildId))]
    public GuildEntity Guild { get; set; }

    #endregion // Navigation properties

    #endregion // Properties
}