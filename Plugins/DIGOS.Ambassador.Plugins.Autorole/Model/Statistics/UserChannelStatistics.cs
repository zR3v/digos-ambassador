//
//  UserChannelStatistics.cs
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

using System.ComponentModel.DataAnnotations.Schema;
using DIGOS.Ambassador.Core.Database.Entities;
using Discord;
using JetBrains.Annotations;

namespace DIGOS.Ambassador.Plugins.Autorole.Model.Statistics
{
    /// <summary>
    /// Represents a set of per-channel statistics for a user in a server.
    /// </summary>
    [PublicAPI]
    [Table("UserChannelStatistics", Schema = "AutoroleModule")]
    public class UserChannelStatistics : EFEntity
    {
        /// <summary>
        /// Gets the Discord channel ID of the relevant channel.
        /// </summary>
        public long ChannelID { get; private set; }

        /// <summary>
        /// Gets the message count in this channel.
        /// </summary>
        public long MessageCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserChannelStatistics"/> class.
        /// </summary>
        /// <param name="channelID">The channel ID.</param>
        /// <param name="messageCount">The message count.</param>
        [UsedImplicitly]
        protected UserChannelStatistics(long channelID, long messageCount)
        {
            this.ChannelID = channelID;
            this.MessageCount = messageCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserChannelStatistics"/> class.
        /// </summary>
        /// <param name="textChannel">The channel.</param>
        /// <param name="messageCount">The message count.</param>
        public UserChannelStatistics(ITextChannel textChannel, long messageCount)
            : this((long)textChannel.Id, messageCount)
        {
        }
    }
}
