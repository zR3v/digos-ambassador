//
//  CharacterCommands.SetCommands.cs
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DIGOS.Ambassador.Discord;
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Characters.Model;
using DIGOS.Ambassador.Plugins.Characters.Permissions;
using DIGOS.Ambassador.Plugins.Characters.Services;
using DIGOS.Ambassador.Plugins.Core.Preconditions;
using DIGOS.Ambassador.Plugins.Permissions.Preconditions;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;
using PermissionTarget = DIGOS.Ambassador.Plugins.Permissions.Model.PermissionTarget;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Characters.CommandModules
{
    public partial class CharacterCommands
    {
        /// <summary>
        /// Property setter commands for characters.
        /// </summary>
        [UsedImplicitly]
        [Group("set")]
        public class SetCommands : ModuleBase
        {
            private readonly DiscordService _discord;
            private readonly UserFeedbackService _feedback;
            private readonly CharacterDiscordService _characters;
            private readonly CharacterRoleService _characterRoles;

            /// <summary>
            /// Initializes a new instance of the <see cref="SetCommands"/> class.
            /// </summary>
            /// <param name="discordService">The Discord integration service.</param>
            /// <param name="feedbackService">The feedback service.</param>
            /// <param name="characterService">The character service.</param>
            /// <param name="characterRoles">The character role service.</param>
            public SetCommands
            (
                DiscordService discordService,
                UserFeedbackService feedbackService,
                CharacterDiscordService characterService,
                CharacterRoleService characterRoles
            )
            {
                _discord = discordService;
                _feedback = feedbackService;
                _characters = characterService;
                _characterRoles = characterRoles;
            }

            /// <summary>
            /// Sets the name of a character.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="newCharacterName">The new name of the character.</param>
            [UsedImplicitly]
            [Command("name")]
            [Summary("Sets the name of a character.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterNameAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                string newCharacterName
            )
            {
                var setNameResult = await _characters.SetCharacterNameAsync(character, newCharacterName);
                if (!setNameResult.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, setNameResult.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Character name set.");
            }

            /// <summary>
            /// Sets the avatar of a character. You can attach an image instead of passing a url as a parameter.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="newCharacterAvatarUrl">The url of the new avatar. Optional.</param>
            [UsedImplicitly]
            [Command("avatar")]
            [Summary("Sets the avatar of a character. You can attach an image instead of passing a url as a parameter.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterAvatarAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                string? newCharacterAvatarUrl = null
            )
            {
                if (newCharacterAvatarUrl is null)
                {
                    if (!this.Context.Message.Attachments.Any())
                    {
                        await _feedback.SendErrorAsync(this.Context, "You need to attach an image or provide a url.");
                        return;
                    }

                    var newAvatar = this.Context.Message.Attachments.First();
                    newCharacterAvatarUrl = newAvatar.Url;
                }

                var galleryImage = character.Images.FirstOrDefault
                (
                    i => i.Name.ToLower().Equals(newCharacterAvatarUrl.ToLower())
                );

                if (!(galleryImage is null))
                {
                    newCharacterAvatarUrl = galleryImage.Url;
                }

                var result = await _characters.SetCharacterAvatarAsync
                (
                    character,
                    newCharacterAvatarUrl ?? throw new ArgumentNullException(nameof(newCharacterAvatarUrl))
                );

                if (!result.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, result.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Character avatar set.");
            }

            /// <summary>
            /// Sets the nickname that the user should have when a character is active.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="newCharacterNickname">The new nickname of the character. Max 32 characters.</param>
            [UsedImplicitly]
            [Alias("nickname", "nick")]
            [Command("nickname")]
            [Summary("Sets the nickname that the user should have when the character is active.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterNicknameAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                string newCharacterNickname
            )
            {
                var setNickResult = await _characters.SetCharacterNicknameAsync
                (
                    (IGuildUser)this.Context.User,
                    character,
                    newCharacterNickname
                );

                if (!setNickResult.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, setNickResult.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Character nickname set.");
            }

            /// <summary>
            /// Sets the summary of a character.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="newCharacterSummary">The new summary. Max 240 characters.</param>
            [UsedImplicitly]
            [Command("summary")]
            [Summary("Sets the summary of a character.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterSummaryAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                string newCharacterSummary
            )
            {
                var setSummaryResult = await _characters.SetCharacterSummaryAsync(character, newCharacterSummary);
                if (!setSummaryResult.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, setSummaryResult.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Character summary set.");
            }

            /// <summary>
            /// Sets the description of a character. You can attach a plaintext document instead of passing the contents as a parameter.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="newCharacterDescription">The new description of the character. Optional.</param>
            [UsedImplicitly]
            [Alias("description", "desc")]
            [Command("description")]
            [Summary("Sets the description of a character. You can attach a plaintext document instead of passing the contents as a parameter.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterDescriptionAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                string? newCharacterDescription = null
            )
            {
                if (newCharacterDescription is null)
                {
                    if (!this.Context.Message.Attachments.Any())
                    {
                        await _feedback.SendErrorAsync(this.Context, "You need to attach a plaintext document or provide an in-message description.");
                        return;
                    }

                    var newDescription = this.Context.Message.Attachments.First();
                    var getAttachmentStreamResult = await _discord.GetAttachmentStreamAsync(newDescription);
                    if (!getAttachmentStreamResult.IsSuccess)
                    {
                        await _feedback.SendErrorAsync(this.Context, getAttachmentStreamResult.ErrorReason);
                        return;
                    }

                    using var sr = new StreamReader(getAttachmentStreamResult.Entity);
                    newCharacterDescription = await sr.ReadToEndAsync();
                }

                var setDescriptionResult = await _characters.SetCharacterDescriptionAsync(character, newCharacterDescription);
                if (!setDescriptionResult.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, setDescriptionResult.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Character description set.");
            }

            /// <summary>
            /// Sets whether or not a character is NSFW.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="isNSFW">Whether or not the character is NSFW.</param>
            [UsedImplicitly]
            [Command("nsfw")]
            [Summary("Sets whether or not a character is NSFW.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterIsNSFWAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                bool isNSFW
            )
            {
                var setNSFW = await _characters.SetCharacterIsNSFWAsync(character, isNSFW);
                if (!setNSFW.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, setNSFW.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, $"Character set to {(isNSFW ? "NSFW" : "SFW")}.");
            }

            /// <summary>
            /// Sets the preferred pronoun for a character.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="pronounFamily">The pronoun family.</param>
            [UsedImplicitly]
            [Alias("pronoun", "pronouns")]
            [Command("pronoun")]
            [Summary("Sets the preferred pronoun of a character.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterPronounAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                string pronounFamily
            )
            {
                var result = await _characters.SetCharacterPronounsAsync(character, pronounFamily);
                if (!result.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, result.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Preferred pronoun set.");
            }

            /// <summary>
            /// Sets the given character's display role.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="discordRole">The role.</param>
            [UsedImplicitly]
            [Command("role")]
            [Summary("Sets the given character's display role.")]
            [RequireContext(ContextType.Guild)]
            [RequirePermission(typeof(EditCharacter), PermissionTarget.Self)]
            public async Task SetCharacterRoleAsync
            (
                [RequireEntityOwnerOrPermission(typeof(EditCharacter), PermissionTarget.Other)]
                Character character,
                IRole discordRole
            )
            {
                var getRoleResult = await _characterRoles.GetCharacterRoleAsync(discordRole);
                if (!getRoleResult.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, getRoleResult.ErrorReason);
                    return;
                }

                var commandInvoker = (IGuildUser)this.Context.User;

                var role = getRoleResult.Entity;
                if (role.Access == RoleAccess.Restricted)
                {
                    if (!commandInvoker.GuildPermissions.ManageRoles)
                    {
                        await _feedback.SendErrorAsync
                        (
                            this.Context,
                            "That role is restricted, and you must be able to manage roles to use it."
                        );

                        return;
                    }
                }

                var owner = await this.Context.Guild.GetUserAsync((ulong)character.Owner.DiscordID);
                if (owner is null)
                {
                    await _feedback.SendErrorAsync(this.Context, "Failed to get the owner as a guild user.");
                    return;
                }

                var setRoleResult = await _characterRoles.SetCharacterRoleAsync(owner, character, role);
                if (!setRoleResult.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, setRoleResult.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Character role set.");
            }

            /// <summary>
            /// Sets your default form to your current character.
            /// </summary>
            [UsedImplicitly]
            [Command("default")]
            [Summary("Sets your default form to your current character.")]
            [RequireContext(ContextType.Guild)]
            public async Task SetDefaultCharacterAsync()
            {
                var result = await _characters.GetCurrentCharacterAsync((IGuildUser)this.Context.User);
                if (!result.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, result.ErrorReason);
                    return;
                }

                await SetDefaultCharacterAsync(result.Entity);
            }

            /// <summary>
            /// Sets your default form to the given character.
            /// </summary>
            /// <param name="character">The character to set as the default character.</param>
            [UsedImplicitly]
            [Command("default")]
            [Summary("Sets your default form to the given character.")]
            [RequireContext(ContextType.Guild)]
            public async Task SetDefaultCharacterAsync
            (
                [RequireEntityOwner]
                Character character
            )
            {
                var result = await _characters.SetDefaultCharacterAsync((IGuildUser)this.Context.User, character);
                if (!result.IsSuccess)
                {
                    await _feedback.SendErrorAsync(this.Context, result.ErrorReason);
                    return;
                }

                await _feedback.SendConfirmationAsync(this.Context, "Default character set.");
            }
        }
    }
}
