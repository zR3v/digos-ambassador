﻿//
//  AmbyCommands.cs
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
using DIGOS.Ambassador.Discord.Extensions;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Amby.Services;
using Discord;
using Discord.Commands;
using Discord.Net;
using JetBrains.Annotations;

using static Discord.Commands.ContextType;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Amby.CommandModules
{
    /// <summary>
    /// Assorted commands that don't really fit anywhere - just for fun, testing, etc.
    /// </summary>
    [UsedImplicitly]
    [Name("amby")]
    [Summary("Assorted commands that don't really fit anywhere - just for fun, testing, etc.")]
    public class AmbyCommands : ModuleBase
    {
        private readonly PortraitService _portraits;
        private readonly SassService _sass;

        private readonly UserFeedbackService _feedback;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbyCommands"/> class.
        /// </summary>
        /// <param name="feedback">The user feedback service.</param>
        /// <param name="sass">The sass service.</param>
        /// <param name="portraits">The portrait service.</param>
        public AmbyCommands
        (
            UserFeedbackService feedback,
            SassService sass,
            PortraitService portraits
        )
        {
            _feedback = feedback;
            _sass = sass;
            _portraits = portraits;
        }

        /// <summary>
        /// Instructs Amby to contact you over DM.
        /// </summary>
        [UsedImplicitly]
        [Command("contact")]
        [Summary("Instructs Amby to contact you over DM.")]
        [RequireContext(Guild)]
        public async Task ContactSelfAsync() => await ContactUserAsync(this.Context.User);

        /// <summary>
        /// Instructs Amby to contact a user over DM.
        /// </summary>
        /// <param name="discordUser">The user to contact.</param>
        [UsedImplicitly]
        [Command("contact")]
        [Summary("Instructs Amby to contact a user over DM.")]
        [RequireContext(Guild)]
        [RequireUserPermission(GuildPermission.MentionEveryone)]
        public async Task ContactUserAsync(IUser discordUser)
        {
            if (discordUser.Id == this.Context.Client.CurrentUser.Id)
            {
                await _feedback.SendErrorAsync(this.Context, "That's a splendid idea - at least then, I'd get an intelligent reply.");
                return;
            }

            if (discordUser.IsBot)
            {
                await _feedback.SendErrorAsync(this.Context, "I could do that, but I doubt I'd get a reply.");
                return;
            }

            var eb = _feedback.CreateFeedbackEmbed
            (
                discordUser,
                Color.DarkPurple,
                $"Hello there, {discordUser.Mention}. I've been instructed to initiate... negotiations... with you. \nA good place to start would be the \"!help <topic>\" command."
            );

            var userDMChannel = await discordUser.GetOrCreateDMChannelAsync();
            try
            {
                await userDMChannel.SendMessageAsync(string.Empty, false, eb);
            }
            catch (HttpException hex) when (hex.WasCausedByDMsNotAccepted())
            {
                return;
            }
            finally
            {
                await userDMChannel.CloseAsync();
            }

            await _feedback.SendConfirmationAsync(this.Context, "User contacted.");
        }

        /// <summary>
        /// Sasses the user in a DIGOS fashion.
        /// </summary>
        [UsedImplicitly]
        [Command("sass")]
        [Summary("Sasses the user in a DIGOS fashion.")]
        public async Task SassAsync()
        {
            var isNsfwChannel = this.Context.Channel is ITextChannel textChannel && textChannel.IsNsfw;
            var getSassResult = await _sass.GetSassAsync(isNsfwChannel);
            if (!getSassResult.IsSuccess)
            {
                await _feedback.SendConfirmationAsync(this.Context, getSassResult.ErrorReason);
                return;
            }

            var sass = getSassResult.Entity;

            await _feedback.SendConfirmationAsync(this.Context, sass);
        }

        /// <summary>
        /// Bweh! A silly command.
        /// </summary>
        [UsedImplicitly]
        [Command("bweh")]
        [Summary("Bweh!")]
        public async Task BwehAsync()
        {
            var eb = _feedback.CreateEmbedBase();
            eb.WithImageUrl(_portraits.BwehUri.ToString());

            await _feedback.SendEmbedAsync(this.Context.Channel, eb.Build());
        }

        /// <summary>
        /// Boops the invoking user.
        /// </summary>
        [UsedImplicitly]
        [Command("boop")]
        [Summary("Boops you.")]
        public async Task BoopAsync()
        {
            await _feedback.SendConfirmationAsync(this.Context, "*boop*");
        }

        /// <summary>
        /// Baps the invoking user.
        /// </summary>
        [UsedImplicitly]
        [Command("bap")]
        [Summary("Baps you.")]
        public async Task BapAsync()
        {
            await _feedback.SendConfirmationAsync(this.Context, "**baps**");
        }

        /// <summary>
        /// Boops the target user.
        /// </summary>
        /// <param name="target">The target.</param>
        [UsedImplicitly]
        [Command("boop")]
        [Summary("Boops the user.")]
        public async Task BoopAsync(IUser target)
        {
            if (target.IsMe(this.Context.Client))
            {
                await _feedback.SendConfirmationAsync(this.Context, "...seriously?");
                await _feedback.SendConfirmationAsync(this.Context, $"*boops {this.Context.User.Mention}*");

                return;
            }

            await _feedback.SendConfirmationAsync(this.Context, $"*boops {target.Mention}*");
        }

        /// <summary>
        /// Baps the target user.
        /// </summary>
        /// <param name="target">The target.</param>
        [UsedImplicitly]
        [Command("bap")]
        [Summary("Baps the user.")]
        public async Task BapAsync(IUser target)
        {
            if (target.IsMe(this.Context.Client))
            {
                await _feedback.SendConfirmationAsync(this.Context, "...seriously?");
                await _feedback.SendConfirmationAsync(this.Context, $"**baps {this.Context.User.Mention}**");

                return;
            }

            await _feedback.SendConfirmationAsync(this.Context, $"**baps {target.Mention}**");
        }

        /// <summary>
        /// Shows some information about Amby's metaworkings.
        /// </summary>
        [UsedImplicitly]
        [Alias("info", "information", "about")]
        [Command("info")]
        [Summary("Shows some information about Amby's metaworkings.")]
        public async Task InfoAsync()
        {
            var eb = _feedback.CreateEmbedBase();

            eb.WithAuthor(this.Context.Client.CurrentUser);
            eb.WithTitle("The DIGOS Ambassador (\"Amby\")");
            eb.WithImageUrl(_portraits.AmbyPortraitUri.ToString());

            eb.WithDescription
            (
                "Amby is a Discord bot written in C# using the Discord.Net and EF Core frameworks. As an ambassador for " +
                "the DIGOS community, she provides a number of useful services for communities with similar interests - " +
                "namely, roleplaying, transformation, weird and wonderful sexual kinks, and much more.\n" +
                "\n" +
                "Amby is free and open source software, licensed under the AGPLv3. All of her source code can be freely " +
                "viewed and improved on Github at https://github.com/Nihlus/DIGOS.Ambassador. You are free to " +
                "run your own instance of Amby, redistribute her code, and modify it to your heart's content. If you're " +
                "not familiar with the AGPL, an excellent summary is available here: " +
                "https://choosealicense.com/licenses/agpl-3.0/.\n" +
                "\n" +
                "Any bugs you encounter should be reported on Github, following the issue template provided there. The " +
                "same holds for feature requests, for which a separate template is provided. Contributions in the form " +
                "of code, artwork, bug triaging, or quality control testing is always greatly appreciated!\n" +
                "\n" +
                "Stay sharky~\n" +
                "- Amby"
            );

            await _feedback.SendPrivateEmbedAsync(this.Context, this.Context.User, eb.Build());
        }
    }
}
