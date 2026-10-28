using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Catalogue.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UniqueProgramNamePerUniversity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_programs_university_id",
                schema: "catalogue",
                table: "programs");

            migrationBuilder.CreateIndex(
                name: "ix_programs_university_id_name",
                schema: "catalogue",
                table: "programs",
                columns: new[] { "university_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_programs_university_id_name",
                schema: "catalogue",
                table: "programs");

            migrationBuilder.CreateIndex(
                name: "ix_programs_university_id",
                schema: "catalogue",
                table: "programs",
                column: "university_id");
        }
    }
}
