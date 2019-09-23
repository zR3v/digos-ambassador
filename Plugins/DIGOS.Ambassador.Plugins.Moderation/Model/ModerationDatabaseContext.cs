//
//  ModerationDatabaseContext.cs
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

using System.Diagnostics.CodeAnalysis;
using DIGOS.Ambassador.Core.Database;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace DIGOS.Ambassador.Plugins.Moderation.Model
{
    /// <summary>
    /// Represents the database model of the dossier plugin.
    /// </summary>
    [PublicAPI]
    public class ModerationDatabaseContext : SchemaAwareDbContext
    {
        private const string SchemaName = "ModerationModule";

        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationDatabaseContext"/> class.
        /// </summary>
        /// <param name="contextOptions">The context options.</param>
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized", Justification = "Initialized by EF Core.")]
        public ModerationDatabaseContext(DbContextOptions<ModerationDatabaseContext> contextOptions)
            : base(SchemaName, contextOptions)
        {
        }
    }
}
