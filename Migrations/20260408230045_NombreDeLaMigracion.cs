using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goal2026API.Migrations
{
    /// <inheritdoc />
    public partial class NombreDeLaMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageKey",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImageUpdatedAtUtc",
                table: "Groups",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ImageKey",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ImageUpdatedAtUtc",
                table: "Groups");
        }
    }
}
