﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGOS.Ambassador.Migrations
{
    public partial class AddDedicatedRPChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DedicatedRoleplayChannelsCategory",
                table: "Servers",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ActiveChannelID",
                table: "Roleplays",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<long>(
                name: "DedicatedChannelID",
                table: "Roleplays",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DedicatedRoleplayChannelsCategory",
                table: "Servers");

            migrationBuilder.DropColumn(
                name: "DedicatedChannelID",
                table: "Roleplays");

            migrationBuilder.AlterColumn<long>(
                name: "ActiveChannelID",
                table: "Roleplays",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}
