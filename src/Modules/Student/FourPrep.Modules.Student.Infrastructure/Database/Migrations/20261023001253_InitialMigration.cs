using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Student.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "student");

            migrationBuilder.CreateTable(
                name: "students",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    school = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    grade = table.Column<int>(type: "integer", nullable: true),
                    region = table.Column<int>(type: "integer", nullable: true),
                    mentor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    link_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_students", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_goals",
                schema: "student",
                columns: table => new
                {
                    exam = table.Column<int>(type: "integer", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_score = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_goals", x => new { x.student_id, x.exam });
                    table.ForeignKey(
                        name: "fk_exam_goals_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "student",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_parents",
                schema: "student",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    linked_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_parents", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_parents_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "student",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_parent_user_id",
                schema: "student",
                table: "student_parents",
                column: "parent_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_student_id_parent_user_id",
                schema: "student",
                table: "student_parents",
                columns: new[] { "student_id", "parent_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_link_code",
                schema: "student",
                table: "students",
                column: "link_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_mentor_user_id",
                schema: "student",
                table: "students",
                column: "mentor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_students_user_id",
                schema: "student",
                table: "students",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_goals",
                schema: "student");

            migrationBuilder.DropTable(
                name: "student_parents",
                schema: "student");

            migrationBuilder.DropTable(
                name: "students",
                schema: "student");
        }
    }
}
