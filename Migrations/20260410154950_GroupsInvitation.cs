using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goal2026API.Migrations
{
    /// <inheritdoc />
    public partial class GroupsInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_GroupId_InvitedUserId_IsDeleted",
                table: "GroupInvitations");

            migrationBuilder.AlterColumn<int>(
                name: "InvitedUserId",
                table: "GroupInvitations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AcceptedByUserId",
                table: "GroupInvitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeclinedByUserId",
                table: "GroupInvitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "GroupInvitations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "InvitedEmail",
                table: "GroupInvitations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvitedEmailNormalized",
                table: "GroupInvitations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "GroupInvitations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_AcceptedByUserId",
                table: "GroupInvitations",
                column: "AcceptedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_DeclinedByUserId",
                table: "GroupInvitations",
                column: "DeclinedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_GroupId_InvitedEmailNormalized_IsDeleted",
                table: "GroupInvitations",
                columns: new[] { "GroupId", "InvitedEmailNormalized", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_InvitedEmailNormalized",
                table: "GroupInvitations",
                column: "InvitedEmailNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_TokenHash",
                table: "GroupInvitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupInvitations_Users_AcceptedByUserId",
                table: "GroupInvitations",
                column: "AcceptedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupInvitations_Users_DeclinedByUserId",
                table: "GroupInvitations",
                column: "DeclinedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupInvitations_Users_AcceptedByUserId",
                table: "GroupInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupInvitations_Users_DeclinedByUserId",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_AcceptedByUserId",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_DeclinedByUserId",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_GroupId_InvitedEmailNormalized_IsDeleted",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_InvitedEmailNormalized",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_TokenHash",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "AcceptedByUserId",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "DeclinedByUserId",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "InvitedEmail",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "InvitedEmailNormalized",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "GroupInvitations");

            migrationBuilder.AlterColumn<int>(
                name: "InvitedUserId",
                table: "GroupInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_GroupId_InvitedUserId_IsDeleted",
                table: "GroupInvitations",
                columns: new[] { "GroupId", "InvitedUserId", "IsDeleted" });
        }
    }
}
