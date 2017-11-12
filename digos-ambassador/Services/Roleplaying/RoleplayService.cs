﻿//
//  RoleplayService.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DIGOS.Ambassador.Database;
using DIGOS.Ambassador.Database.Roleplaying;
using DIGOS.Ambassador.Database.UserInfo;
using DIGOS.Ambassador.Extensions;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace DIGOS.Ambassador.Services.Roleplaying
{
	/// <summary>
	/// Acts as an interface for accessing, enabling, and disabling ongoing roleplays.
	/// </summary>
	public class RoleplayService
	{
		private readonly CommandService Commands;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoleplayService"/> class.
		/// </summary>
		/// <param name="commands">The application's command service.</param>
		public RoleplayService(CommandService commands)
		{
			this.Commands = commands;
		}

		/// <summary>
		/// Consumes a message, adding it to the active roleplay in its channel if the author is a participant.
		/// </summary>
		/// <param name="message">The message to consume.</param>
		public async void ConsumeMessage([NotNull]IMessage message)
		{
			using (var db = new GlobalInfoContext())
			{
				var result = await GetActiveRoleplayAsync(db, message.Channel);
				if (!result.IsSuccess)
				{
					return;
				}

				var roleplay = result.Entity;

				await AddToOrUpdateMessageInRoleplay(db, roleplay, message);
			}
		}

		/// <summary>
		/// Creates a roleplay with the given parameters.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="context">The context of the command.</param>
		/// <param name="roleplayName">The name of the roleplay.</param>
		/// <param name="roleplaySummary">The summary of the roleplay.</param>
		/// <param name="isNSFW">Whether or not the roleplay is NSFW.</param>
		/// <param name="isPublic">Whether or not the roleplay is public.</param>
		/// <returns>A creation result which may or may not have been successful.</returns>
		public async Task<CreateEntityResult<Roleplay>> CreateRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[NotNull] string roleplayName,
			[NotNull] string roleplaySummary,
			bool isNSFW,
			bool isPublic)
		{
			var owner = await db.GetOrRegisterUserAsync(context.Message.Author);
			var roleplay = new Roleplay
			{
				IsActive = false,
				ActiveChannelID = context.Channel.Id,
				Owner = owner,
				Participants = new List<User> { owner },
				Messages = new List<UserMessage>()
			};

			var setNameResult = await SetRoleplayNameAsync(db, context, roleplay, roleplayName);
			if (!setNameResult.IsSuccess)
			{
				return CreateEntityResult<Roleplay>.FromError(setNameResult);
			}

			var setSummaryResult = await SetRoleplaySummaryAsync(db, roleplay, roleplaySummary);
			if (!setSummaryResult.IsSuccess)
			{
				return CreateEntityResult<Roleplay>.FromError(setSummaryResult);
			}

			var setIsNSFWResult = await SetRoleplayIsNSFWAsync(db, roleplay, isNSFW);
			if (!setIsNSFWResult.IsSuccess)
			{
				return CreateEntityResult<Roleplay>.FromError(setIsNSFWResult);
			}

			var setIsPublicResult = await SetRoleplayIsPublicAsync(db, roleplay, isPublic);
			if (!setIsPublicResult.IsSuccess)
			{
				return CreateEntityResult<Roleplay>.FromError(setIsPublicResult);
			}

			await db.Roleplays.AddAsync(roleplay);
			await db.SaveChangesAsync();

			var roleplayResult = await GetUserRoleplayByNameAsync(db, context, context.Message.Author, roleplayName);
			if (!roleplayResult.IsSuccess)
			{
				return CreateEntityResult<Roleplay>.FromError(roleplayResult);
			}

			return CreateEntityResult<Roleplay>.FromSuccess(roleplayResult.Entity);
		}

		/// <summary>
		/// Adds a new message to the given roleplay, or edits it if there is an existing one.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="roleplay">The roleplay to modify.</param>
		/// <param name="message">The message to add or update.</param>
		/// <returns>A task wrapping the update action.</returns>
		public async Task<ModifyEntityResult> AddToOrUpdateMessageInRoleplay
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] Roleplay roleplay,
			[NotNull] IMessage message
		)
		{
			if (roleplay.Participants == null || !roleplay.Participants.Any(p => p.DiscordID == message.Author.Id))
			{
				return ModifyEntityResult.FromError(CommandError.Unsuccessful, "The given message was not authored by a participant of the roleplay.");
			}

			string userNick = message.Author.Username;
			if (message.Author is SocketGuildUser guildUser && !string.IsNullOrEmpty(guildUser.Nickname))
			{
				userNick = guildUser.Nickname;
			}

			if (roleplay.Messages.Any(m => m.DiscordMessageID == message.Id))
			{
				// Edit the existing message
				var existingMessage = roleplay.Messages.Find(m => m.DiscordMessageID == message.Id);

				if (existingMessage.Contents.Equals(message.Content))
				{
					return ModifyEntityResult.FromError(CommandError.Unsuccessful, "Nothing to do; message content match.");
				}

				existingMessage.Contents = message.Content;

				await db.SaveChangesAsync();
				return ModifyEntityResult.FromSuccess(ModifyEntityAction.Edited);
			}

			var roleplayMessage = await UserMessage.FromDiscordMessageAsync(message, userNick);
			roleplay.Messages.Add(roleplayMessage);

			await db.SaveChangesAsync();
			return ModifyEntityResult.FromSuccess(ModifyEntityAction.Added);
		}

		/// <summary>
		/// This method searches for the best matching roleplay given an owner and a name. If no owner is provided, then
		/// the global list is searched for a unique name. If neither are provided, the currently active roleplay is
		/// returned. If no match can be found, a failed result is returned.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="context">The command context.</param>
		/// <param name="roleplayOwner">The owner of the roleplay, if any.</param>
		/// <param name="roleplayName">The name of the roleplay, if any.</param>
		/// <returns>A retrieval result which may or may not have succeeded.</returns>
		public async Task<RetrieveEntityResult<Roleplay>> GetBestMatchingRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[CanBeNull] IUser roleplayOwner,
			[CanBeNull] string roleplayName
		)
		{
			if (roleplayOwner is null && roleplayName is null)
			{
				return await GetActiveRoleplayAsync(db, context.Channel);
			}

			if (roleplayOwner is null)
			{
				return await GetNamedRoleplayAsync(db, roleplayName);
			}

			if (roleplayName.IsNullOrWhitespace())
			{
				return RetrieveEntityResult<Roleplay>.FromError(CommandError.ObjectNotFound, "Roleplays can't have empty or null names.");
			}

			return await GetUserRoleplayByNameAsync(db, context, roleplayOwner, roleplayName);
		}

		/// <summary>
		/// Gets a roleplay by its given name.
		/// </summary>
		/// <param name="db">The database context where the data is stored.</param>
		/// <param name="roleplayName">The name of the roleplay.</param>
		/// <returns>A retrieval result which may or may not have succeeded.</returns>
		public async Task<RetrieveEntityResult<Roleplay>> GetNamedRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] string roleplayName
		)
		{
			if (await db.Roleplays.CountAsync(rp => rp.Name.Equals(roleplayName, StringComparison.OrdinalIgnoreCase)) > 1)
			{
				return RetrieveEntityResult<Roleplay>.FromError
				(
					CommandError.MultipleMatches,
					"There's more than one roleplay with that name. Please specify which user it belongs to."
				);
			}

			var roleplay = db.Roleplays
				.Include(rp => rp.Owner)
				.Include(rp => rp.Participants)
				.Include(rp => rp.Messages)
				.Include(rp => rp.KickedUsers)
				.Include(rp => rp.InvitedUsers)
				.FirstOrDefault(rp => rp.Name.Equals(roleplayName, StringComparison.OrdinalIgnoreCase));

			if (roleplay is null)
			{
				return RetrieveEntityResult<Roleplay>.FromError(CommandError.ObjectNotFound, "No roleplay with that name found.");
			}

			return RetrieveEntityResult<Roleplay>.FromSuccess(roleplay);
		}

		/// <summary>
		/// Gets the current active roleplay in the given channel.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="channel">The channel to get the roleplay from.</param>
		/// <returns>A retrieval result which may or may not have succeeded.</returns>
		public async Task<RetrieveEntityResult<Roleplay>> GetActiveRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] IChannel channel
		)
		{
			var roleplay = await db.Roleplays
				.Include(rp => rp.Owner)
				.Include(rp => rp.Participants)
				.Include(rp => rp.Messages)
				.Include(rp => rp.KickedUsers)
				.Include(rp => rp.InvitedUsers)
				.FirstOrDefaultAsync(rp => rp.IsActive && rp.ActiveChannelID == channel.Id);

			if (roleplay is null)
			{
				return RetrieveEntityResult<Roleplay>.FromError(CommandError.ObjectNotFound, "There is no roleplay that is currently active in this channel.");
			}

			return RetrieveEntityResult<Roleplay>.FromSuccess(roleplay);
		}

		/// <summary>
		/// Determines whether or not there is an active roleplay in the given channel.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="channel">The channel to check.</param>
		/// <returns>true if there is an active roleplay; otherwise, false.</returns>
		public async Task<bool> HasActiveRoleplayAsync([NotNull] GlobalInfoContext db, [NotNull] IChannel channel)
		{
			return await db.Roleplays.AnyAsync(rp => rp.IsActive && rp.ActiveChannelID == channel.Id);
		}

		/// <summary>
		/// Determines whether or not the given roleplay name is unique for a given user.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="discordUser">The user to check.</param>
		/// <param name="roleplayName">The roleplay name to check.</param>
		/// <returns>true if the name is unique; otherwise, false.</returns>
		public async Task<bool> IsRoleplayNameUniqueForUserAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] IUser discordUser,
			[NotNull] string roleplayName
		)
		{
			var userRoleplays = GetUserRoleplays(db, discordUser);
			if (await userRoleplays.CountAsync() <= 0)
			{
				return true;
			}

			return !await userRoleplays.AnyAsync(rp => rp.Name.Equals(roleplayName, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Get the roleplays owned by the given user.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="discordUser">The user to get the roleplays of.</param>
		/// <returns>A queryable list of roleplays belonging to the user.</returns>
		[NotNull]
		[ItemNotNull]
		public IQueryable<Roleplay> GetUserRoleplays([NotNull]GlobalInfoContext db, [NotNull]IUser discordUser)
		{
			return db.Roleplays
				.Include(rp => rp.Owner)
				.Include(rp => rp.Participants)
				.Include(rp => rp.Messages)
				.Include(rp => rp.KickedUsers)
				.Include(rp => rp.InvitedUsers)
				.Where(rp => rp.Owner.DiscordID == discordUser.Id);
		}

		/// <summary>
		/// Gets a roleplay belonging to a given user by a given name.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="context">The context of the user.</param>
		/// <param name="roleplayOwner">The user to get the roleplay from.</param>
		/// <param name="roleplayName">The name of the roleplay.</param>
		/// <returns>A retrieval result which may or may not have succeeded.</returns>
		public async Task<RetrieveEntityResult<Roleplay>> GetUserRoleplayByNameAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[NotNull] IUser roleplayOwner,
			[NotNull] string roleplayName
		)
		{
			var roleplay = await db.Roleplays
			.Include(rp => rp.Owner)
			.Include(rp => rp.Participants)
			.Include(rp => rp.Messages)
			.Include(rp => rp.KickedUsers)
			.Include(rp => rp.InvitedUsers)
			.FirstOrDefaultAsync
			(
				rp =>
					rp.Name.Equals(roleplayName, StringComparison.OrdinalIgnoreCase) &&
					rp.Owner.DiscordID == roleplayOwner.Id
			);

			if (roleplay is null)
			{
				var isCurrentUser = context.Message.Author.Id == roleplayOwner.Id;
				var errorMessage = isCurrentUser
					? "You don't own a roleplay with that name."
					: "The user doesn't own a roleplay with that name.";

				return RetrieveEntityResult<Roleplay>.FromError(CommandError.ObjectNotFound, errorMessage);
			}

			return RetrieveEntityResult<Roleplay>.FromSuccess(roleplay);
		}

		/// <summary>
		/// Kicks the given user from the given roleplay.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="context">The context of the user.</param>
		/// <param name="roleplay">The roleplay to remove the user from.</param>
		/// <param name="kickedUser">The user to remove from the roleplay.</param>
		/// <returns>An execution result which may or may not have succeeded.</returns>
		public async Task<ExecuteResult> KickUserFromRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[NotNull] Roleplay roleplay,
			[NotNull] IUser kickedUser
		)
		{
			var removeUserResult = await RemoveUserFromRoleplayAsync(db, context, roleplay, kickedUser);
			if (!removeUserResult.IsSuccess)
			{
				return removeUserResult;
			}

			roleplay.KickedUsers.RemoveAll(ku => ku.DiscordID == kickedUser.Id);
			await db.SaveChangesAsync();

			return ExecuteResult.FromSuccess();
		}

		/// <summary>
		/// Removes the given user from the given roleplay.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="context">The context of the user.</param>
		/// <param name="roleplay">The roleplay to remove the user from.</param>
		/// <param name="removedUser">The user to remove from the roleplay.</param>
		/// <returns>An execution result which may or may not have succeeded.</returns>
		public async Task<ExecuteResult> RemoveUserFromRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[NotNull] Roleplay roleplay,
			[NotNull] IUser removedUser
		)
		{
			var isCurrentUser = context.Message.Author.Id == removedUser.Id;
			if (!roleplay.Participants.Any(p => p.DiscordID == removedUser.Id))
			{
				var errorMessage = isCurrentUser
					? "You're not in that roleplay."
					: "No matching user found in the roleplay.";

				return ExecuteResult.FromError(CommandError.Unsuccessful, errorMessage);
			}

			if (roleplay.Owner.DiscordID == removedUser.Id)
			{
				var errorMessage = isCurrentUser
					? "You can't leave a roleplay you own."
					: "The owner of a roleplay can't be removed from it.";

				return ExecuteResult.FromError(CommandError.Unsuccessful, errorMessage);
			}

			roleplay.Participants = roleplay.Participants.Where(p => p.DiscordID != removedUser.Id).ToList();
			await db.SaveChangesAsync();

			return ExecuteResult.FromSuccess();
		}

		/// <summary>
		/// Adds the given user to the given roleplay.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="context">The context of the user.</param>
		/// <param name="roleplay">The roleplay to add the user to.</param>
		/// <param name="newUser">The user to add to the roleplay.</param>
		/// <returns>An execution result which may or may not have succeeded.</returns>
		public async Task<ExecuteResult> AddUserToRoleplayAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[NotNull] Roleplay roleplay,
			[NotNull] IUser newUser
		)
		{
			var isCurrentUser = context.Message.Author.Id == newUser.Id;
			if (roleplay.Participants.Any(p => p.DiscordID == newUser.Id))
			{
				var errorMessage = isCurrentUser
					? "You're already in that roleplay."
					: "The user is aleady in that roleplay.";

				return ExecuteResult.FromError(CommandError.Unsuccessful, errorMessage);
			}

			if (roleplay.KickedUsers.Any(ku => ku.DiscordID == newUser.Id))
			{
				var errorMessage = isCurrentUser
					? "You've been kicked from that roleplay, and can't rejoin unless invited."
					: "The user has been kicked from that roleplay, and can't rejoin unless invited.";

				return ExecuteResult.FromError(CommandError.UnmetPrecondition, errorMessage);
			}

			// Check the invite list for nonpublic roleplays.
			if (!roleplay.IsPublic && !roleplay.InvitedUsers.Any(iu => iu.DiscordID == newUser.Id))
			{
				var errorMessage = isCurrentUser
					? "You haven't been invited to that roleplay."
					: "The user hasn't been invited to that roleplay.";

				return ExecuteResult.FromError(CommandError.UnmetPrecondition, errorMessage);
			}

			if (!roleplay.IsPublic)
			{
				// Remove the user from the invite list
				roleplay.InvitedUsers.RemoveAll(iu => iu.DiscordID == newUser.Id);
			}

			roleplay.Participants.Add(await db.GetOrRegisterUserAsync(newUser));
			await db.SaveChangesAsync();

			return ExecuteResult.FromSuccess();
		}

		/// <summary>
		/// Invites the user to the given roleplay.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="roleplay">The roleplay to invite the user to.</param>
		/// <param name="invitedUser">The user to invite.</param>
		/// <returns>An execution result which may or may not have succeeded.</returns>
		public async Task<ExecuteResult> InviteUserAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] Roleplay roleplay,
			[NotNull] IUser invitedUser
		)
		{
			if (roleplay.IsPublic)
			{
				return ExecuteResult.FromError(CommandError.UnmetPrecondition, "The roleplay is not set to private.");
			}

			if (roleplay.InvitedUsers.Any(p => p.DiscordID == invitedUser.Id))
			{
				return ExecuteResult.FromError(CommandError.Unsuccessful, "The user has already been invited to that roleplay.");
			}

			// Remove the invited user from the kick list, if they're on it
			roleplay.KickedUsers.RemoveAll(ku => ku.DiscordID == invitedUser.Id);
			roleplay.InvitedUsers.Add(await db.GetOrRegisterUserAsync(invitedUser));
			await db.SaveChangesAsync();

			return ExecuteResult.FromSuccess();
		}

		/// <summary>
		/// Transfers ownership of the named roleplay to the specified user.
		/// </summary>
		/// <param name="db">The database where the roleplays are stored.</param>
		/// <param name="newOwner">The new owner.</param>
		/// <param name="roleplay">The roleplay to transfer.</param>
		/// <returns>An execution result which may or may not have succeeded.</returns>
		public async Task<ExecuteResult> TransferRoleplayOwnershipAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] IUser newOwner,
			[NotNull] Roleplay roleplay
		)
		{
			if (roleplay.Owner.DiscordID == newOwner.Id)
			{
				return ExecuteResult.FromError(CommandError.Unsuccessful, "That person already owns the roleplay.");
			}

			if (GetUserRoleplays(db, newOwner).Any(rp => rp.Name.Equals(roleplay.Name, StringComparison.OrdinalIgnoreCase)))
			{
				return ExecuteResult.FromError(CommandError.MultipleMatches, $"That user already owns a roleplay named {roleplay.Name}. Please rename it first.");
			}

			var newUser = await db.GetOrRegisterUserAsync(newOwner);
			roleplay.Owner = newUser;

			await db.SaveChangesAsync();

			return ExecuteResult.FromSuccess();
		}

		/// <summary>
		/// Sets the name of the given roleplay.
		/// </summary>
		/// <param name="db">The database containing the roleplays.</param>
		/// <param name="context">The context of the operation.</param>
		/// <param name="roleplay">The roleplay to set the name of.</param>
		/// <param name="newRoleplayName">The new name.</param>
		/// <returns>A modification result which may or may not have succeeded.</returns>
		public async Task<ModifyEntityResult> SetRoleplayNameAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] SocketCommandContext context,
			[NotNull] Roleplay roleplay,
			[NotNull] string newRoleplayName
		)
		{
			var isCurrentUser = context.Message.Author.Id == roleplay.Owner.DiscordID;
			if (string.IsNullOrWhiteSpace(newRoleplayName))
			{
				return ModifyEntityResult.FromError(CommandError.BadArgCount, "You need to provide a name.");
			}

			if (!await IsRoleplayNameUniqueForUserAsync(db, context.Message.Author, newRoleplayName))
			{
				var errorMessage = isCurrentUser
					? "You already have a roleplay with that name."
					: "The user already has a roleplay with that name.";

				return ModifyEntityResult.FromError(CommandError.MultipleMatches, errorMessage);
			}

			if (!IsRoleplayNameValid(newRoleplayName))
			{
				return ModifyEntityResult.FromError(CommandError.UnmetPrecondition, "The given name is not valid.");
			}

			roleplay.Name = newRoleplayName;
			await db.SaveChangesAsync();

			return ModifyEntityResult.FromSuccess(ModifyEntityAction.Edited);
		}

		/// <summary>
		/// Sets the summary of the given roleplay.
		/// </summary>
		/// <param name="db">The database containing the roleplays.</param>
		/// <param name="roleplay">The roleplay to set the summary of.</param>
		/// <param name="newRoleplaySummary">The new summary.</param>
		/// <returns>A modification result which may or may not have succeeded.</returns>
		public async Task<ModifyEntityResult> SetRoleplaySummaryAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] Roleplay roleplay,
			[NotNull] string newRoleplaySummary
		)
		{
			if (string.IsNullOrWhiteSpace(newRoleplaySummary))
			{
				return ModifyEntityResult.FromError(CommandError.BadArgCount, "You need to provide a new summary.");
			}

			roleplay.Summary = newRoleplaySummary;
			await db.SaveChangesAsync();

			return ModifyEntityResult.FromSuccess(ModifyEntityAction.Edited);
		}

		/// <summary>
		/// Sets whether or not a roleplay is NSFW.
		/// </summary>
		/// <param name="db">The database containing the roleplays.</param>
		/// <param name="roleplay">The roleplay to set the value in.</param>
		/// <param name="isNSFW">The new value.</param>
		/// <returns>A modification result which may or may not have succeeded.</returns>
		public async Task<ModifyEntityResult> SetRoleplayIsNSFWAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] Roleplay roleplay,
			bool isNSFW
		)
		{
			if (roleplay.Messages.Count > 0 && roleplay.IsNSFW && !isNSFW)
			{
				return ModifyEntityResult.FromError(CommandError.UnmetPrecondition, "You can't mark a NSFW roleplay with messages in it as non-NSFW.");
			}

			roleplay.IsNSFW = isNSFW;
			await db.SaveChangesAsync();

			return ModifyEntityResult.FromSuccess(ModifyEntityAction.Edited);
		}

		/// <summary>
		/// Sets whether or not a roleplay is public.
		/// </summary>
		/// <param name="db">The database containing the roleplays.</param>
		/// <param name="roleplay">The roleplay to set the value in.</param>
		/// <param name="isPublic">The new value.</param>
		/// <returns>A modification result which may or may not have succeeded.</returns>
		public async Task<ModifyEntityResult> SetRoleplayIsPublicAsync
		(
			[NotNull] GlobalInfoContext db,
			[NotNull] Roleplay roleplay,
			bool isPublic
		)
		{
			roleplay.IsPublic = isPublic;
			await db.SaveChangesAsync();

			return ModifyEntityResult.FromSuccess(ModifyEntityAction.Edited);
		}

		/// <summary>
		/// Builds a list of the command names and aliases in this module, and checks that the given roleplay name is
		/// not one of them.
		/// </summary>
		/// <param name="roleplayName">The name of the roleplay.</param>
		/// <returns>true if the name is valid; otherwise, false.</returns>
		[ContractAnnotation("roleplayName:null => false")]
		public bool IsRoleplayNameValid([CanBeNull] string roleplayName)
		{
			if (roleplayName.IsNullOrWhitespace())
			{
				return false;
			}

			var commandModule = this.Commands.Modules.First(m => m.Name == "roleplay");
			var submodules = commandModule.Submodules;

			var commandNames = commandModule.Commands.SelectMany(c => c.Aliases);
			commandNames = commandNames.Union(commandModule.Commands.Select(c => c.Name));

			var submoduleCommandNames = submodules.SelectMany(s => s.Commands.SelectMany(c => c.Aliases));
			submoduleCommandNames = submoduleCommandNames.Union(submodules.SelectMany(s => s.Commands.Select(c => c.Name)));

			commandNames = commandNames.Union(submoduleCommandNames);

			return !commandNames.Contains(roleplayName);
		}
	}
}