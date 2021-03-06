//
//  AutoroleUpdateBehaviour.cs
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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Autorole.Model;
using DIGOS.Ambassador.Plugins.Autorole.Results;
using DIGOS.Ambassador.Plugins.Autorole.Services;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.Behaviours;

namespace DIGOS.Ambassador.Plugins.Autorole.Behaviours
{
    /// <summary>
    /// Performs continuous updates of autoroles, as well as notifications to a specific channel about users that
    /// require confirmation.
    /// </summary>
    [UsedImplicitly]
    public class AutoroleUpdateBehaviour : ContinuousDiscordBehaviour<AutoroleUpdateBehaviour>
    {
        private readonly UserFeedbackService _feedback;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoroleUpdateBehaviour"/> class.
        /// </summary>
        /// <param name="client">The discord client.</param>
        /// <param name="serviceScope">The service scope.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="feedback">The user feedback service.</param>
        public AutoroleUpdateBehaviour
        (
            DiscordSocketClient client,
            IServiceScope serviceScope,
            ILogger<AutoroleUpdateBehaviour> logger,
            UserFeedbackService feedback
        )
            : base(client, serviceScope, logger)
        {
            _feedback = feedback;
        }

        /// <inheritdoc />
        protected override async Task OnTickAsync(CancellationToken ct, IServiceProvider tickServices)
        {
            var autoroles = tickServices.GetRequiredService<AutoroleService>();
            var autoroleUpdates = tickServices.GetRequiredService<AutoroleUpdateService>();

            foreach (var autorole in await autoroles.GetAutoroles().ToListAsync(ct))
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                var guild = this.Client.GetGuild((ulong)autorole.Server.DiscordID);
                if (guild is null)
                {
                    continue;
                }

                var botUser = guild.GetUser(this.Client.CurrentUser.Id);
                if (botUser is null)
                {
                    // The client is probably not ready yet.
                    break;
                }

                if (!botUser.GuildPermissions.ManageRoles)
                {
                    // It's pointless to try to add or remove roles on this server
                    continue;
                }

                if (!guild.HasAllMembers)
                {
                    await guild.DownloadUsersAsync();
                }

                foreach (var user in guild.Users.Where(u => !u.IsBot).Where(u => !u.IsWebhook))
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    var updateResult = await autoroleUpdates.UpdateAutoroleForUserAsync(autorole, user);
                    if (!updateResult.IsSuccess)
                    {
                        this.Log.LogError(updateResult.Exception, updateResult.ErrorReason);
                        continue;
                    }

                    switch (updateResult.Status)
                    {
                        case AutoroleUpdateStatus.RequiresAffirmation:
                        {
                            await NotifyUserNeedsAffirmation(autoroles, guild, autorole, user);
                            break;
                        }
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }

        private async Task NotifyUserNeedsAffirmation
        (
            AutoroleService autoroles,
            SocketGuild guild,
            AutoroleConfiguration autorole,
            IUser user
        )
        {
            var getAutoroleConfirmation = await autoroles.GetOrCreateAutoroleConfirmationAsync(autorole, user);
            if (!getAutoroleConfirmation.IsSuccess)
            {
                this.Log.LogError(getAutoroleConfirmation.Exception, getAutoroleConfirmation.ErrorReason);
                return;
            }

            var autoroleConfirmation = getAutoroleConfirmation.Entity;

            if (autoroleConfirmation.HasNotificationBeenSent)
            {
                return;
            }

            var getSettings = await autoroles.GetOrCreateServerSettingsAsync(guild);
            if (!getSettings.IsSuccess)
            {
                this.Log.LogError(getSettings.Exception, getSettings.ErrorReason);
                return;
            }

            var settings = getSettings.Entity;

            var notificationChannelID = settings.AffirmationRequiredNotificationChannelID;
            if (notificationChannelID is null)
            {
                return;
            }

            var notificationChannel = guild.GetTextChannel((ulong)notificationChannelID.Value);
            if (notificationChannel is null)
            {
                return;
            }

            var embed = _feedback.CreateEmbedBase()
                .WithTitle("Confirmation Required")
                .WithDescription
                (
                    $"{MentionUtils.MentionUser(user.Id)} has met the requirements for the " +
                    $"{MentionUtils.MentionRole((ulong)autorole.DiscordRoleID)} role.\n" +
                    $"\n" +
                    $"Use \"!at affirm {MentionUtils.MentionRole((ulong)autorole.DiscordRoleID)} " +
                    $"{MentionUtils.MentionUser(user.Id)}\" to affirm and give the user the role."
                )
                .WithColor(Color.Green);

            try
            {
                await _feedback.SendEmbedAsync(notificationChannel, embed.Build());

                autoroleConfirmation.HasNotificationBeenSent = true;
                await autoroles.SaveChangesAsync();
            }
            catch (HttpException hex) when (hex.WasCausedByMissingPermission())
            {
            }
        }
    }
}
