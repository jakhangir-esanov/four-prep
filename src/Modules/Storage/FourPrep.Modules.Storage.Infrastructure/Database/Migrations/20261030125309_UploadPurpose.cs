using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Storage.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UploadPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "purpose",
                schema: "storage",
                table: "stored_files",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                "UPDATE storage.stored_files SET purpose = 2 " +
                "WHERE public_key IS NOT NULL AND content_type IN ('image/jpeg', 'image/png');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "purpose",
                schema: "storage",
                table: "stored_files");
        }
    }
}
