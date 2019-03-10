﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGOS.Ambassador.Migrations
{
    public partial class RequireCharactersToHaveCurrentAppearances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Appearance_CurrentAppearanceID",
                table: "Characters");

            migrationBuilder.AlterColumn<long>(
                name: "CurrentAppearanceID",
                table: "Characters",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Appearance_CurrentAppearanceID",
                table: "Characters",
                column: "CurrentAppearanceID",
                principalTable: "Appearance",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Appearance_CurrentAppearanceID",
                table: "Characters");

            migrationBuilder.AlterColumn<long>(
                name: "CurrentAppearanceID",
                table: "Characters",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Appearance_CurrentAppearanceID",
                table: "Characters",
                column: "CurrentAppearanceID",
                principalTable: "Appearance",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
