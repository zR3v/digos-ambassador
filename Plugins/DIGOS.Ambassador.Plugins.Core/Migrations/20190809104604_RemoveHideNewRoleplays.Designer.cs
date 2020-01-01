﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using System.Diagnostics.CodeAnalysis;
using System;
using DIGOS.Ambassador.Plugins.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DIGOS.Ambassador.Plugins.Core.Migrations
{
    [DbContext(typeof(CoreDatabaseContext))]
    [Migration("20190809104604_RemoveHideNewRoleplays")]
    partial class RemoveHideNewRoleplays
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Core")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Servers.Server", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<long>("DiscordID");

                    b.Property<bool>("IsNSFW");

                    b.Property<string>("JoinMessage");

                    b.Property<bool>("SendJoinMessage");

                    b.Property<bool>("SuppressPermissonWarnings");

                    b.HasKey("ID");

                    b.ToTable("Servers","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.User", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Bio");

                    b.Property<long>("DiscordID");

                    b.Property<long?>("ServerID");

                    b.Property<int?>("Timezone");

                    b.HasKey("ID");

                    b.HasIndex("ServerID");

                    b.ToTable("Users","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.UserConsent", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("DiscordID");

                    b.Property<bool>("HasConsented");

                    b.HasKey("ID");

                    b.ToTable("UserConsents","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.User", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Servers.Server")
                        .WithMany("KnownUsers")
                        .HasForeignKey("ServerID");
                });
#pragma warning restore 612, 618
        }
    }
}
