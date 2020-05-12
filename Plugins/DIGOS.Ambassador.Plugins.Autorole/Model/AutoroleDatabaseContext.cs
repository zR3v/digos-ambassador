//
//  AutoroleDatabaseContext.cs
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

using DIGOS.Ambassador.Plugins.Autorole.Model.Conditions;
using DIGOS.Ambassador.Plugins.Autorole.Model.Conditions.Bases;
using DIGOS.Ambassador.Plugins.Autorole.Model.Statistics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Remora.EntityFrameworkCore.Modular;

// ReSharper disable RedundantDefaultMemberInitializer - suppressions for indirectly initialized properties.
namespace DIGOS.Ambassador.Plugins.Autorole.Model
{
    /// <summary>
    /// Represents the database model of the dossier plugin.
    /// </summary>
    [PublicAPI]
    public class AutoroleDatabaseContext : SchemaAwareDbContext
    {
        private const string SchemaName = "AutoroleModule";

        /// <summary>
        /// Gets or sets the table where autoroles are stored.
        /// </summary>
        public DbSet<Autorole> Autoroles { get; [UsedImplicitly] set; } = null!;

        /// <summary>
        /// Gets or sets the table where user statistics are stored.
        /// </summary>
        public DbSet<UserStatistics> UserStatistics { get; [UsedImplicitly] set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoroleDatabaseContext"/> class.
        /// </summary>
        /// <param name="contextOptions">The context options.</param>
        public AutoroleDatabaseContext(DbContextOptions<AutoroleDatabaseContext> contextOptions)
            : base(SchemaName, contextOptions)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Abstract bases
            modelBuilder.Entity<TimeSinceEventCondition>()
                .HasBaseType<AutoroleCondition>();

            modelBuilder.Entity<MessageCountInSourceCondition>()
                .HasBaseType<AutoroleCondition>();

            // Concrete types
            modelBuilder.Entity<ReactionCondition>()
                .HasBaseType<AutoroleCondition>();

            modelBuilder.Entity<RoleCondition>()
                .HasBaseType<AutoroleCondition>();

            modelBuilder.Entity<TimeSinceJoinCondition>()
                .HasBaseType<TimeSinceEventCondition>();

            modelBuilder.Entity<TimeSinceLastActivityCondition>()
                .HasBaseType<TimeSinceEventCondition>();

            modelBuilder.Entity<MessageCountInChannelCondition>()
                .HasBaseType<MessageCountInSourceCondition>();

            modelBuilder.Entity<MessageCountInGuildCondition>()
                .HasBaseType<MessageCountInSourceCondition>();
        }
    }
}
