using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Academy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "academy");

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "academy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "jsonb", nullable: false),
                    description = table.Column<string>(type: "jsonb", nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "class_groups",
                schema: "academy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    entry_level = table.Column<string>(type: "jsonb", nullable: false),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    weeks = table.Column<int>(type: "integer", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    teacher_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_class_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_class_groups_courses_course_id",
                        column: x => x.course_id,
                        principalSchema: "academy",
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "schedule_slots",
                schema: "academy",
                columns: table => new
                {
                    day = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    class_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_slots", x => new { x.class_group_id, x.day, x.start_time });
                    table.ForeignKey(
                        name: "fk_schedule_slots_class_groups_class_group_id",
                        column: x => x.class_group_id,
                        principalSchema: "academy",
                        principalTable: "class_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_class_groups_code",
                schema: "academy",
                table: "class_groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_class_groups_course_id",
                schema: "academy",
                table: "class_groups",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_class_groups_status_starts_on",
                schema: "academy",
                table: "class_groups",
                columns: new[] { "status", "starts_on" });

            migrationBuilder.CreateIndex(
                name: "ix_class_groups_teacher_user_id",
                schema: "academy",
                table: "class_groups",
                column: "teacher_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_courses_slug",
                schema: "academy",
                table: "courses",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_slots",
                schema: "academy");

            migrationBuilder.DropTable(
                name: "class_groups",
                schema: "academy");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "academy");
        }
    }
}
