using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Storage.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class PublicFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "public_key",
                schema: "storage",
                table: "stored_files",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "published_on_utc",
                schema: "storage",
                table: "stored_files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_stored_files_public_key",
                schema: "storage",
                table: "stored_files",
                column: "public_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stored_files_public_key",
                schema: "storage",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "public_key",
                schema: "storage",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "published_on_utc",
                schema: "storage",
                table: "stored_files");
        }
    }
}
