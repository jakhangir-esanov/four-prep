using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Student.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InternationalStudentLocation : Migration
    {
        // The old region column held the Region enum (Uzbekistan's regions, 1..14).
        // Existing values become country UZ plus the region's name.
        private const string CopyUzbekRegionsSql =
            """
            UPDATE student.students
            SET country = 'UZ',
                region_name = CASE region
                    WHEN 1 THEN 'Tashkent City'
                    WHEN 2 THEN 'Tashkent Region'
                    WHEN 3 THEN 'Andijan'
                    WHEN 4 THEN 'Bukhara'
                    WHEN 5 THEN 'Fergana'
                    WHEN 6 THEN 'Jizzakh'
                    WHEN 7 THEN 'Kashkadarya'
                    WHEN 8 THEN 'Khorezm'
                    WHEN 9 THEN 'Namangan'
                    WHEN 10 THEN 'Navoi'
                    WHEN 11 THEN 'Samarkand'
                    WHEN 12 THEN 'Surkhandarya'
                    WHEN 13 THEN 'Syrdarya'
                    WHEN 14 THEN 'Karakalpakstan'
                END
            WHERE region IS NOT NULL;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                schema: "student",
                table: "students",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "region_name",
                schema: "student",
                table: "students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(CopyUzbekRegionsSql);

            migrationBuilder.DropColumn(
                name: "region",
                schema: "student",
                table: "students");

            migrationBuilder.RenameColumn(
                name: "region_name",
                schema: "student",
                table: "students",
                newName: "region");

            migrationBuilder.CreateIndex(
                name: "ix_students_country",
                schema: "student",
                table: "students",
                column: "country");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_students_country",
                schema: "student",
                table: "students");

            // Free-text regions have no enum value to go back to.
            migrationBuilder.DropColumn(
                name: "region",
                schema: "student",
                table: "students");

            migrationBuilder.DropColumn(
                name: "country",
                schema: "student",
                table: "students");

            migrationBuilder.AddColumn<int>(
                name: "region",
                schema: "student",
                table: "students",
                type: "integer",
                nullable: true);
        }
    }
}
