//
//  ModerationService.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Threading.Tasks;
using DIGOS.Ambassador.Core.Results;
using DIGOS.Ambassador.Plugins.Core.Services.Servers;
using DIGOS.Ambassador.Plugins.Moderation.Model;
using Discord;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace DIGOS.Ambassador.Plugins.Moderation.Services
{
    /// <summary>
    /// Acts as an interface for accessing and modifying moderation actions.
    /// </summary>
    [PublicAPI]
    public sealed class ModerationService
    {
        [NotNull] private readonly ModerationDatabaseContext _database;
        [NotNull] private readonly ServerService _servers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationService"/> class.
        /// </summary>
        /// <param name="database">The database context.</param>
        /// <param name="servers">The server service.</param>
        public ModerationService
        (
            [NotNull] ModerationDatabaseContext database,
            [NotNull] ServerService servers
        )
        {
            _database = database;
            _servers = servers;
        }

        /// <summary>
        /// Gets or creates the settings entity for the given Discord guild.
        /// </summary>
        /// <param name="discordServer">The server.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<RetrieveEntityResult<ServerModerationSettings>> GetOrCreateServerSettingsAsync
        (
            [NotNull] IGuild discordServer
        )
        {
            var getExistingEntry = await GetServerSettingsAsync(discordServer);
            if (getExistingEntry.IsSuccess)
            {
                return getExistingEntry.Entity;
            }

            var createSettings = await CreateServerSettingsAsync(discordServer);
            if (!createSettings.IsSuccess)
            {
                return RetrieveEntityResult<ServerModerationSettings>.FromError(createSettings);
            }

            return createSettings.Entity;
        }

        /// <summary>
        /// Gets the settings for the given Discord guild.
        /// </summary>
        /// <param name="discordServer">The server.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<RetrieveEntityResult<ServerModerationSettings>> GetServerSettingsAsync
        (
            [NotNull] IGuild discordServer
        )
        {
            var entity = await _database.ServerSettings.FirstOrDefaultAsync
            (
                s => s.Server.DiscordID == (long)discordServer.Id
            );

            if (entity is null)
            {
                return RetrieveEntityResult<ServerModerationSettings>.FromError
                (
                    "The server doesn't have any settings."
                );
            }

            return entity;
        }

        /// <summary>
        /// Creates the settings for the given Discord guild.
        /// </summary>
        /// <param name="discordServer">The server.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<ServerModerationSettings>> CreateServerSettingsAsync
        (
            [NotNull] IGuild discordServer
        )
        {
            var existingEntity = await GetServerSettingsAsync(discordServer);
            if (existingEntity.IsSuccess)
            {
                return CreateEntityResult<ServerModerationSettings>.FromError("That server already has settings.");
            }

            var getServer = await _servers.GetOrRegisterServerAsync(discordServer);
            if (!getServer.IsSuccess)
            {
                return CreateEntityResult<ServerModerationSettings>.FromError(getServer);
            }

            var server = getServer.Entity;
            var settings = new ServerModerationSettings(server);

            _database.ServerSettings.Update(settings);
            await _database.SaveChangesAsync();

            return settings;
        }
    }
}
