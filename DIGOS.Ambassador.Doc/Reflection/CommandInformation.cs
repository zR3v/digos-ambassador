//
//  CommandInformation.cs
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DIGOS.Ambassador.Doc.Extensions;
using JetBrains.Annotations;
using Mono.Cecil;

namespace DIGOS.Ambassador.Doc.Reflection
{
    /// <summary>
    /// Represents information about a Discord command module.
    /// </summary>
    [PublicAPI]
    public class CommandInformation
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the summary of the command.
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Gets the aliases of the command, if any.
        /// </summary>
        public IReadOnlyCollection<string> Aliases { get; }

        /// <summary>
        /// Gets the parameters of the command, if any.
        /// </summary>
        public IReadOnlyCollection<ParameterDefinition> Parameters { get; }

        /// <summary>
        /// Gets the module that the command is defined in.
        /// </summary>
        public ModuleInformation Module { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInformation"/> class.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="summary">The summary of the command.</param>
        /// <param name="aliases">The aliases of the command, if any.</param>
        /// <param name="parameters">The parameters of the command, if any.</param>
        /// <param name="module">The module the command belongs to.</param>
        private CommandInformation
        (
            string name,
            string summary,
            List<string> aliases,
            ParameterDefinition[] parameters,
            ModuleInformation module
        )
        {
            this.Name = name;
            this.Summary = summary;
            this.Aliases = aliases;
            this.Parameters = parameters;
            this.Module = module;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInformation"/> class.
        /// </summary>
        /// <param name="commandMethod">The type definition of the command method.</param>
        /// <param name="module">The module the command is defined in.</param>
        /// <param name="information">The created information.</param>
        /// <returns>true if the information was successfully created; otherwise, false.</returns>
        [Pure]
        public static bool TryCreate
        (
            MethodDefinition commandMethod,
            ModuleInformation module,
            [NotNullWhen(true)] out CommandInformation? information
        )
        {
            information = null;

            if (!commandMethod.TryGetCommandName(out var name))
            {
                return false;
            }

            if (!commandMethod.TryGetSummary(out var summary))
            {
                summary = string.Empty;
            }

            if (!commandMethod.TryGetAliases(out var aliases))
            {
                aliases = new string[] { };
            }

            var allAliases = new List<string> { name };
            allAliases.AddRange(aliases);

            allAliases = allAliases.Distinct().ToList();

            var parameters = commandMethod.HasParameters
                ? commandMethod.Parameters.ToArray()
                : new ParameterDefinition[] { };

            information = new CommandInformation
            (
                name,
                summary,
                allAliases,
                parameters,
                module
            );

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }
    }
}
