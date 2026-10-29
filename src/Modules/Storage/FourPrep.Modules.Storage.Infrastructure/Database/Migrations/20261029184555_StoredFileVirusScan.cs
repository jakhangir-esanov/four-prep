using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Storage.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class StoredFileVirusScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "scan_attempts",
                schema: "storage",
                table: "stored_files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "scan_status",
                schema: "storage",
                table: "stored_files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "scanned_on_utc",
                schema: "storage",
                table: "stored_files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sha256",
                schema: "storage",
                table: "stored_files",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_stored_files_status_scan_status",
                schema: "storage",
                table: "stored_files",
                columns: new[] { "status", "scan_status" });

            migrationBuilder.Sql("UPDATE storage.stored_files SET scan_status = 1 WHERE status = 2 AND scan_status = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stored_files_status_scan_status",
                schema: "storage",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "scan_attempts",
                schema: "storage",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "scan_status",
                schema: "storage",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "scanned_on_utc",
                schema: "storage",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "sha256",
                schema: "storage",
                table: "stored_files");
        }
    }
}
