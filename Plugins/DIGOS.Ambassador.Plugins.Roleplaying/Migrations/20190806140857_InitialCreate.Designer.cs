﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using System;
using DIGOS.Ambassador.Plugins.Roleplaying.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DIGOS.Ambassador.Plugins.Roleplaying.Migrations
{
    [DbContext(typeof(RoleplayingDatabaseContext))]
    [Migration("20190806140857_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("RoleplayModule")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Core.Model.Users.User", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Bio");

                    b.Property<int>("Class");

                    b.Property<long>("DiscordID");

                    b.Property<bool>("HideNewRoleplays");

                    b.Property<int?>("Timezone");

                    b.HasKey("ID");

                    b.ToTable("Users","Core");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Roleplaying.Model.Roleplay", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ActiveChannelID");

                    b.Property<long?>("DedicatedChannelID");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsNSFW");

                    b.Property<bool>("IsPublic");

                    b.Property<DateTime?>("LastUpdated");

                    b.Property<string>("Name");

                    b.Property<long>("OwnerID");

                    b.Property<long>("ServerID");

                    b.Property<string>("Summary");

                    b.HasKey("ID");

                    b.HasIndex("OwnerID");

                    b.ToTable("Roleplays","RoleplayModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Roleplaying.Model.RoleplayParticipant", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("RoleplayID");

                    b.Property<int>("Status");

                    b.Property<long>("UserID");

                    b.HasKey("ID");

                    b.HasIndex("RoleplayID");

                    b.HasIndex("UserID");

                    b.ToTable("RoleplayParticipants","RoleplayModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Roleplaying.Model.UserMessage", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AuthorDiscordID");

                    b.Property<string>("AuthorNickname");

                    b.Property<string>("Contents");

                    b.Property<long>("DiscordMessageID");

                    b.Property<long?>("RoleplayID");

                    b.Property<DateTimeOffset>("Timestamp");

                    b.HasKey("ID");

                    b.HasIndex("RoleplayID");

                    b.ToTable("UserMessages","RoleplayModule");
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Roleplaying.Model.Roleplay", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Users.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Roleplaying.Model.RoleplayParticipant", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Roleplaying.Model.Roleplay", "Roleplay")
                        .WithMany("ParticipatingUsers")
                        .HasForeignKey("RoleplayID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DIGOS.Ambassador.Plugins.Core.Model.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DIGOS.Ambassador.Plugins.Roleplaying.Model.UserMessage", b =>
                {
                    b.HasOne("DIGOS.Ambassador.Plugins.Roleplaying.Model.Roleplay")
                        .WithMany("Messages")
                        .HasForeignKey("RoleplayID");
                });
#pragma warning restore 612, 618
        }
    }
}
