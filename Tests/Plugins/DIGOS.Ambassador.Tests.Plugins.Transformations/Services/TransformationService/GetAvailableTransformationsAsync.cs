//
//  GetAvailableTransformationsAsync.cs
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

#pragma warning disable SA1600
#pragma warning disable CS1591
#pragma warning disable SA1649

using System.Threading.Tasks;
using DIGOS.Ambassador.Plugins.Transformations.Transformations;
using Xunit;

namespace DIGOS.Ambassador.Tests.Plugins.Transformations
{
    public partial class TransformationServiceTests
    {
        public class GetAvailableTransformationsAsync : TransformationServiceTestBase
        {
            [Fact]
            public async Task ReturnsNonemptySetForExistingTransformation()
            {
                var result = await this.Transformations.GetAvailableTransformationsAsync(Bodypart.Face);

                Assert.NotEmpty(result);
            }

            [Fact]
            public async Task ReturnsEmptySetForNonExistantTransformation()
            {
                var result = await this.Transformations.GetAvailableTransformationsAsync(Bodypart.Wings);

                Assert.Empty(result);
            }
        }
    }
}
