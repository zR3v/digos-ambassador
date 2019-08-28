//
//  ShiftBodypartColourAsync.cs
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

using System.Linq;
using System.Threading.Tasks;
using DIGOS.Ambassador.Plugins.Characters.Model;
using DIGOS.Ambassador.Plugins.Characters.Services;
using DIGOS.Ambassador.Plugins.Characters.Services.Pronouns;
using DIGOS.Ambassador.Plugins.Core.Model.Entity;
using DIGOS.Ambassador.Plugins.Transformations.Model.Appearances;
using DIGOS.Ambassador.Plugins.Transformations.Transformations;
using DIGOS.Ambassador.Tests.Utility;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DIGOS.Ambassador.Tests.Plugins.Transformations
{
    public partial class TransformationServiceTests
    {
        public class ShiftBodypartColourAsync : TransformationServiceTestBase
        {
            private readonly IGuild _guild;

            private readonly IUser _owner = MockHelper.CreateDiscordGuildUser(0);
            private readonly IUser _invoker = MockHelper.CreateDiscordGuildUser(1);

            private readonly Colour _newColour;

            private readonly ICommandContext _context;
            private Character _character;

            private Colour _originalColour;

            private AppearanceConfiguration _appearanceConfiguration;

            public ShiftBodypartColourAsync()
            {
                var mockedGuild = new Mock<IGuild>();
                mockedGuild.Setup(g => g.Id).Returns(1);
                mockedGuild.Setup
                    (
                        c =>
                            c.GetUserAsync
                            (
                                It.Is<ulong>(id => id == _owner.Id),
                                CacheMode.AllowDownload,
                                null
                            )
                    )
                    .Returns(Task.FromResult((IGuildUser)_owner));

                _guild = mockedGuild.Object;

                var mockedContext = new Mock<ICommandContext>();
                mockedContext.Setup(c => c.Guild).Returns(_guild);
                mockedContext.Setup(c => c.User).Returns(_invoker);

                _context = mockedContext.Object;

                Colour.TryParse("bright purple", out _newColour);
            }

            /// <inheritdoc />
            protected override void RegisterServices(IServiceCollection serviceCollection)
            {
                base.RegisterServices(serviceCollection);

                serviceCollection
                    .AddScoped<OwnedEntityService>()
                    .AddScoped<CommandService>()
                    .AddScoped<CharacterService>();
            }

            protected override async Task InitializeTestAsync()
            {
                this.Services.GetRequiredService<PronounService>().WithPronounProvider(new TheyPronounProvider());
                this.Transformations.WithDescriptionBuilder(new TransformationDescriptionBuilder(this.Services));

                // Ensure owner is opted into transformations
                var protection = await this.Transformations.GetOrCreateServerUserProtectionAsync
                (
                    _owner,
                    _guild
                );

                protection.Entity.HasOptedIn = true;

                // Create a test character
                var owner = (await this.Users.GetOrRegisterUserAsync(_owner)).Entity;
                var character = new Character
                {
                    Name = "Test",
                    Owner = owner,
                    PronounProviderFamily = "They"
                };

                this.CharacterDatabase.Characters.Update(character);
                await this.CharacterDatabase.SaveChangesAsync();

                _character = this.CharacterDatabase.Characters.First();

                // Set up the default appearance
                var getAppearanceConfigurationResult = await this.Transformations.GetOrCreateAppearanceConfigurationAsync
                (
                    _character
                );

                _appearanceConfiguration = getAppearanceConfigurationResult.Entity;
                _originalColour = _appearanceConfiguration.GetAppearanceComponent(Bodypart.Face, Chirality.Center).BaseColour;
            }

            [Fact]
            public async Task ReturnsUnsuccessfulResultIfUserIsNotAllowedToTransformTarget()
            {
                await this.Transformations.BlacklistUserAsync(_owner, _invoker);

                var result = await this.Transformations.ShiftBodypartColourAsync
                (
                    _context,
                    _character,
                    Bodypart.Face,
                    _newColour
                );

                Assert.False(result.IsSuccess);
            }

            [Fact]
            public async Task ReturnsUnsuccessfulResultIfCharacterDoesNotHaveBodypart()
            {
                var result = await this.Transformations.ShiftBodypartColourAsync
                (
                    _context,
                    _character,
                    Bodypart.Wing,
                    _newColour,
                    Chirality.Left
                );

                Assert.False(result.IsSuccess);
            }

            [Fact]
            public async Task ReturnsUnsuccessfulResultIfBodypartIsAlreadyThatColour()
            {
                var result = await this.Transformations.ShiftBodypartColourAsync
                (
                    _context,
                    _character,
                    Bodypart.Face,
                    _originalColour
                );

                Assert.False(result.IsSuccess);
            }

            [Fact]
            public async Task CanShiftBodypartColour()
            {
                var result = await this.Transformations.ShiftBodypartColourAsync
                (
                    _context,
                    _character,
                    Bodypart.Face,
                    _newColour
                );

                Assert.True(result.IsSuccess);
            }

            [Fact]
            public async Task ShiftsBodypartIntoCorrectColour()
            {
                await this.Transformations.ShiftBodypartColourAsync
                (
                    _context,
                    _character,
                    Bodypart.Face,
                    _newColour
                );

                var face = _appearanceConfiguration.CurrentAppearance.Components.First(c => c.Bodypart == Bodypart.Face);
                Assert.Same(_newColour, face.BaseColour);
            }

            [Fact]
            public async Task ReturnsShiftMessage()
            {
                var result = await this.Transformations.ShiftBodypartColourAsync
                (
                    _context,
                    _character,
                    Bodypart.Face,
                    _newColour
                );

                Assert.NotNull(result.ShiftMessage);
                Assert.NotEmpty(result.ShiftMessage);
            }
        }
    }
}