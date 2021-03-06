//
//  UserStatisticsService.cs
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

using System.Linq;
using System.Threading.Tasks;
using DIGOS.Ambassador.Plugins.Autorole.Model;
using DIGOS.Ambassador.Plugins.Autorole.Model.Statistics;
using DIGOS.Ambassador.Plugins.Core.Services.Servers;
using DIGOS.Ambassador.Plugins.Core.Services.Users;
using Discord;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace DIGOS.Ambassador.Plugins.Autorole.Services
{
    /// <summary>
    /// Business logic class for user statistics.
    /// </summary>
    public class UserStatisticsService
    {
        private readonly AutoroleDatabaseContext _database;
        private readonly UserService _users;
        private readonly ServerService _servers;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStatisticsService"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="users">The user service.</param>
        /// <param name="servers">The server service.</param>
        public UserStatisticsService(AutoroleDatabaseContext database, UserService users, ServerService servers)
        {
            _database = database;
            _users = users;
            _servers = servers;
        }

        /// <summary>
        /// Gets or creates a set of statistics for a given user.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<UserStatistics>> GetOrCreateUserStatisticsAsync
        (
            IUser discordUser
        )
        {
            var existingStatistics = await _database.UserStatistics.AsQueryable().FirstOrDefaultAsync
            (
                s => s.User.DiscordID == (long)discordUser.Id
            );

            if (!(existingStatistics is null))
            {
                return existingStatistics;
            }

            var getUser = await _users.GetOrRegisterUserAsync(discordUser);
            if (!getUser.IsSuccess)
            {
                return CreateEntityResult<UserStatistics>.FromError(getUser);
            }

            var user = getUser.Entity;

            var newStatistics = _database.CreateProxy<UserStatistics>(user);

            _database.UserStatistics.Update(newStatistics);
            await _database.SaveChangesAsync();

            return newStatistics;
        }

        /// <summary>
        /// Gets or creates a set of per-server statistics for a given user.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<UserServerStatistics>> GetOrCreateUserServerStatisticsAsync
        (
            IGuildUser discordUser
        )
        {
            var getStatistics = await GetOrCreateUserStatisticsAsync(discordUser);
            if (!getStatistics.IsSuccess)
            {
                return CreateEntityResult<UserServerStatistics>.FromError(getStatistics);
            }

            var statistics = getStatistics.Entity;
            var existingServerStatistics = statistics.ServerStatistics.FirstOrDefault
            (
                s => s.Server.DiscordID == (long)discordUser.Guild.Id
            );

            if (!(existingServerStatistics is null))
            {
                return existingServerStatistics;
            }

            var getServer = await _servers.GetOrRegisterServerAsync(discordUser.Guild);
            if (!getServer.IsSuccess)
            {
                return CreateEntityResult<UserServerStatistics>.FromError(getServer);
            }

            var server = getServer.Entity;

            // Since we're adding to the database indirectly, an attach is required here.
            _database.Attach(server);

            var newServerStatistics = _database.CreateProxy<UserServerStatistics>(server);

            statistics.ServerStatistics.Add(newServerStatistics);
            await _database.SaveChangesAsync();

            return newServerStatistics;
        }

        /// <summary>
        /// Gets or creates a set of per-channel statistics for a given user.
        /// </summary>
        /// <param name="discordUser">The user.</param>
        /// <param name="discordChannel">The channel.</param>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<CreateEntityResult<UserChannelStatistics>> GetOrCreateUserChannelStatisticsAsync
        (
            IGuildUser discordUser,
            ITextChannel discordChannel
        )
        {
            var getServerStats = await GetOrCreateUserServerStatisticsAsync(discordUser);
            if (!getServerStats.IsSuccess)
            {
                return CreateEntityResult<UserChannelStatistics>.FromError(getServerStats);
            }

            var serverStats = getServerStats.Entity;
            var existingStats = serverStats.ChannelStatistics.FirstOrDefault
            (
                s => s.ChannelID == (long)discordChannel.Id
            );

            if (!(existingStats is null))
            {
                return existingStats;
            }

            var newStats = _database.CreateProxy<UserChannelStatistics>(discordChannel);
            serverStats.ChannelStatistics.Add(newStats);

            await _database.SaveChangesAsync();
            return newStats;
        }

        /// <summary>
        /// Explicitly saves the database.
        /// </summary>
        /// <returns>The number of entities saved.</returns>
        public Task<int> SaveChangesAsync()
        {
            return _database.SaveChangesAsync();
        }
    }
}
