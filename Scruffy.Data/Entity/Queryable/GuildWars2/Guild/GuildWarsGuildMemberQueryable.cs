﻿using Scruffy.Data.Entity.Queryable.Base;
using Scruffy.Data.Entity.Tables.GuildWars2.Guild;

namespace Scruffy.Data.Entity.Queryable.GuildWars2.Guild;

/// <summary>
/// Queryable for accessing the <see cref="GuildWarsGuildMemberEntity"/>
/// </summary>
public class GuildWarsGuildMemberQueryable : QueryableBase<GuildWarsGuildMemberEntity>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="queryable"><see cref="IQueryable"/>-object</param>
    public GuildWarsGuildMemberQueryable(IQueryable<GuildWarsGuildMemberEntity> queryable)
        : base(queryable)
    {
    }

    #endregion // Constructor
}