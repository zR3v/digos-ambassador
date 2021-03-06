//
//  AutoroleCommands.AutoroleConditionCommands.MessageCountInChannelConditionCommands.cs
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
using DIGOS.Ambassador.Discord.Feedback;
using DIGOS.Ambassador.Plugins.Autorole.Model;
using DIGOS.Ambassador.Plugins.Autorole.Model.Conditions;
using DIGOS.Ambassador.Plugins.Autorole.Permissions;
using DIGOS.Ambassador.Plugins.Autorole.Services;
using DIGOS.Ambassador.Plugins.Permissions.Preconditions;
using Discord;
using Discord.Commands;
using JetBrains.Annotations;
using PermissionTarget = DIGOS.Ambassador.Plugins.Permissions.Model.PermissionTarget;

#pragma warning disable SA1615 // Disable "Element return value should be documented" due to TPL tasks

namespace DIGOS.Ambassador.Plugins.Autorole.CommandModules
{
    public partial class AutoroleCommands
    {
        public partial class AutoroleConditionCommands
        {
            /// <summary>
            /// Contains commands for adding or modifying a condition based on a certain number of messages in a
            /// channel.
            /// </summary>
            [Group("total-messages-in")]
            public class MessageCountInChannelConditionCommands : ModuleBase
            {
                private readonly AutoroleService _autoroles;
                private readonly UserFeedbackService _feedback;

                /// <summary>
                /// Initializes a new instance of the <see cref="MessageCountInChannelConditionCommands"/> class.
                /// </summary>
                /// <param name="autoroles">The autorole service.</param>
                /// <param name="feedback">The feedback service.</param>
                public MessageCountInChannelConditionCommands(AutoroleService autoroles, UserFeedbackService feedback)
                {
                    _autoroles = autoroles;
                    _feedback = feedback;
                }

                /// <summary>
                /// Adds an instance of the condition to the role.
                /// </summary>
                /// <param name="autorole">The autorole configuration.</param>
                /// <param name="channel">The Discord channel.</param>
                /// <param name="count">The message count.</param>
                [UsedImplicitly]
                [Command]
                [Summary("Adds an instance of the condition to the role.")]
                [RequireContext(ContextType.Guild)]
                [RequirePermission(typeof(EditAutorole), PermissionTarget.Self)]
                public async Task AddConditionAsync(AutoroleConfiguration autorole, ITextChannel channel, long count)
                {
                    var condition = _autoroles.CreateConditionProxy<MessageCountInChannelCondition>(channel, count);
                    if (condition is null)
                    {
                        await _feedback.SendErrorAsync(this.Context, "Failed to create a condition object. Yikes!");
                        return;
                    }

                    var addCondition = await _autoroles.AddConditionAsync(autorole, condition);
                    if (!addCondition.IsSuccess)
                    {
                        await _feedback.SendErrorAsync(this.Context, addCondition.ErrorReason);
                        return;
                    }

                    await _feedback.SendConfirmationAsync(this.Context, "Condition added.");
                }

                /// <summary>
                /// Modifies an instance of the condition on the role.
                /// </summary>
                /// <param name="autorole">The autorole configuration.</param>
                /// <param name="conditionID">The ID of the condition.</param>
                /// <param name="channel">The Discord channel.</param>
                /// <param name="count">The message count.</param>
                [UsedImplicitly]
                [Command]
                [Summary("Modifies an instance of the condition on the role.")]
                [RequireContext(ContextType.Guild)]
                [RequirePermission(typeof(EditAutorole), PermissionTarget.Self)]
                public async Task ModifyConditionAsync
                (
                    AutoroleConfiguration autorole,
                    long conditionID,
                    ITextChannel channel,
                    long count
                )
                {
                    var getCondition = _autoroles.GetCondition<MessageCountInChannelCondition>
                    (
                        autorole,
                        conditionID
                    );

                    if (!getCondition.IsSuccess)
                    {
                        await _feedback.SendErrorAsync(this.Context, getCondition.ErrorReason);
                        return;
                    }

                    var condition = getCondition.Entity;
                    condition.RequiredCount = count;
                    condition.SourceID = (long)channel.Id;

                    await _autoroles.SaveChangesAsync();
                    await _feedback.SendConfirmationAsync(this.Context, "Condition updated.");
                }
            }
        }
    }
}
