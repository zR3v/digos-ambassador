﻿// <auto-generated />
#pragma warning disable CS1591
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantUsingDirective
using Microsoft.EntityFrameworkCore.Migrations;

namespace DIGOS.Ambassador.Migrations
{
    public partial class MoveRoleplayEntitiesToSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleplayParticipant_Roleplays_RoleplayID",
                table: "RoleplayParticipant");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleplayParticipant_Users_UserID",
                table: "RoleplayParticipant");

            migrationBuilder.DropForeignKey(
                name: "FK_Roleplays_Users_OwnerID",
                table: "Roleplays");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessage_Roleplays_RoleplayID",
                table: "UserMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMessage",
                table: "UserMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roleplays",
                table: "Roleplays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleplayParticipant",
                table: "RoleplayParticipant");

            migrationBuilder.EnsureSchema(
                name: "RoleplayModule");

            migrationBuilder.RenameTable(
                name: "UserMessage",
                newName: "UserMessages",
                newSchema: "RoleplayModule");

            migrationBuilder.RenameTable(
                name: "Roleplays",
                newName: "RoleplaysIntermediateTableName",
                newSchema: "RoleplayModule");

            migrationBuilder.RenameTable(
                name: "RoleplayParticipant",
                newName: "RoleplayParticipants",
                newSchema: "RoleplayModule");

            migrationBuilder.RenameIndex(
                name: "IX_UserMessage_RoleplayID",
                schema: "RoleplayModule",
                table: "UserMessages",
                newName: "IX_UserMessages_RoleplayID");

            migrationBuilder.RenameIndex(
                name: "IX_Roleplays_OwnerID",
                schema: "RoleplayModule",
                table: "RoleplaysIntermediateTableName",
                newName: "IX_RoleplaysIntermediateTableName_OwnerID");

            migrationBuilder.RenameIndex(
                name: "IX_RoleplayParticipant_UserID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                newName: "IX_RoleplayParticipants_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_RoleplayParticipant_RoleplayID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                newName: "IX_RoleplayParticipants_RoleplayID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMessages",
                schema: "RoleplayModule",
                table: "UserMessages",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleplaysIntermediateTableName",
                schema: "RoleplayModule",
                table: "RoleplaysIntermediateTableName",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleplayParticipants",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplayParticipants_RoleplaysIntermediateTableName_RoleplayID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                column: "RoleplayID",
                principalSchema: "RoleplayModule",
                principalTable: "RoleplaysIntermediateTableName",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplayParticipants_Users_UserID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplaysIntermediateTableName_Users_OwnerID",
                schema: "RoleplayModule",
                table: "RoleplaysIntermediateTableName",
                column: "OwnerID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_RoleplaysIntermediateTableName_RoleplayID",
                schema: "RoleplayModule",
                table: "UserMessages",
                column: "RoleplayID",
                principalSchema: "RoleplayModule",
                principalTable: "RoleplaysIntermediateTableName",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleplayParticipants_RoleplaysIntermediateTableName_RoleplayID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleplayParticipants_Users_UserID",
                schema: "RoleplayModule",
                table: "RoleplayParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleplaysIntermediateTableName_Users_OwnerID",
                schema: "RoleplayModule",
                table: "RoleplaysIntermediateTableName");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_RoleplaysIntermediateTableName_RoleplayID",
                schema: "RoleplayModule",
                table: "UserMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMessages",
                schema: "RoleplayModule",
                table: "UserMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleplaysIntermediateTableName",
                schema: "RoleplayModule",
                table: "RoleplaysIntermediateTableName");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleplayParticipants",
                schema: "RoleplayModule",
                table: "RoleplayParticipants");

            migrationBuilder.RenameTable(
                name: "UserMessages",
                schema: "RoleplayModule",
                newName: "UserMessage");

            migrationBuilder.RenameTable(
                name: "RoleplaysIntermediateTableName",
                schema: "RoleplayModule",
                newName: "Roleplays");

            migrationBuilder.RenameTable(
                name: "RoleplayParticipants",
                schema: "RoleplayModule",
                newName: "RoleplayParticipant");

            migrationBuilder.RenameIndex(
                name: "IX_UserMessages_RoleplayID",
                table: "UserMessage",
                newName: "IX_UserMessage_RoleplayID");

            migrationBuilder.RenameIndex(
                name: "IX_RoleplaysIntermediateTableName_OwnerID",
                table: "Roleplays",
                newName: "IX_Roleplays_OwnerID");

            migrationBuilder.RenameIndex(
                name: "IX_RoleplayParticipants_UserID",
                table: "RoleplayParticipant",
                newName: "IX_RoleplayParticipant_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_RoleplayParticipants_RoleplayID",
                table: "RoleplayParticipant",
                newName: "IX_RoleplayParticipant_RoleplayID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMessage",
                table: "UserMessage",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roleplays",
                table: "Roleplays",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleplayParticipant",
                table: "RoleplayParticipant",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplayParticipant_Roleplays_RoleplayID",
                table: "RoleplayParticipant",
                column: "RoleplayID",
                principalTable: "Roleplays",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleplayParticipant_Users_UserID",
                table: "RoleplayParticipant",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roleplays_Users_OwnerID",
                table: "Roleplays",
                column: "OwnerID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessage_Roleplays_RoleplayID",
                table: "UserMessage",
                column: "RoleplayID",
                principalTable: "Roleplays",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
