﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using System.Diagnostics.CodeAnalysis;
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DIGOS.Ambassador.Plugins.Roleplaying.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Core");

            migrationBuilder.EnsureSchema(
                name: "RoleplayModule");

            migrationBuilder.CreateTable(
                name: "Roleplays",
                schema: "RoleplayModule",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ServerID = table.Column<long>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    IsNSFW = table.Column<bool>(nullable: false),
                    ActiveChannelID = table.Column<long>(nullable: true),
                    DedicatedChannelID = table.Column<long>(nullable: true),
                    OwnerID = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roleplays", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Roleplays_Users_OwnerID",
                        column: x => x.OwnerID,
                        principalSchema: "Core",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleplayParticipants",
                schema: "RoleplayModule",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    RoleplayID = table.Column<long>(nullable: false),
                    UserID = table.Column<long>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleplayParticipants", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RoleplayParticipants_Roleplays_RoleplayID",
                        column: x => x.RoleplayID,
                        principalSchema: "RoleplayModule",
                        principalTable: "Roleplays",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleplayParticipants_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "Core",
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserMessages",
                schema: "RoleplayModule",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordMessageID = table.Column<long>(nullable: false),
                    AuthorDiscordID = table.Column<long>(nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(nullable: false),
                    AuthorNickname = table.Column<string>(nullable: true),
                    Contents = table.Column<string>(nullable: true),
                    RoleplayID = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserMessages_Roleplays_RoleplayID",
                        column: x => x.RoleplayID,
                        principalSchema: "RoleplayModule",
                        principalTable: "Roleplays",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayParticipants_RoleplayID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                column: "RoleplayID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleplayParticipants_UserID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Roleplays_OwnerID",
                schema: "RoleplayModule",
                table: "Roleplays",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_RoleplayID",
                schema: "RoleplayModule",
                table: "UserMessages",
                column: "RoleplayID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleplayParticipants",
                schema: "RoleplayModule");

            migrationBuilder.DropTable(
                name: "UserMessages",
                schema: "RoleplayModule");

            migrationBuilder.DropTable(
                name: "Roleplays",
                schema: "RoleplayModule");
        }
    }
}
