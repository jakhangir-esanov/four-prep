using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Meeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "meeting");

            migrationBuilder.CreateTable(
                name: "meetings",
                schema: "meeting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    student_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    student_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    booked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    starts_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    meeting_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    office_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    office_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    office_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    staff_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    cancelled_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancelled_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meetings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "staff_schedules",
                schema: "meeting",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    meeting_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    office_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    office_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    office_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    last_booked_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_schedules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "availability_windows",
                schema: "meeting",
                columns: table => new
                {
                    day = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    staff_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_availability_windows", x => new { x.staff_schedule_id, x.day, x.mode, x.start_time });
                    table.ForeignKey(
                        name: "fk_availability_windows_staff_schedules_staff_schedule_id",
                        column: x => x.staff_schedule_id,
                        principalSchema: "meeting",
                        principalTable: "staff_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meetings_staff_user_id_starts_on_utc",
                schema: "meeting",
                table: "meetings",
                columns: new[] { "staff_user_id", "starts_on_utc" },
                unique: true,
                filter: "status = 1");

            migrationBuilder.CreateIndex(
                name: "ix_meetings_staff_user_id_status_starts_on_utc",
                schema: "meeting",
                table: "meetings",
                columns: new[] { "staff_user_id", "status", "starts_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_meetings_starts_on_utc",
                schema: "meeting",
                table: "meetings",
                column: "starts_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_meetings_student_user_id_kind",
                schema: "meeting",
                table: "meetings",
                columns: new[] { "student_user_id", "kind" },
                unique: true,
                filter: "status = 1");

            migrationBuilder.CreateIndex(
                name: "ix_meetings_student_user_id_starts_on_utc",
                schema: "meeting",
                table: "meetings",
                columns: new[] { "student_user_id", "starts_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_staff_schedules_staff_user_id",
                schema: "meeting",
                table: "staff_schedules",
                column: "staff_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_windows",
                schema: "meeting");

            migrationBuilder.DropTable(
                name: "meetings",
                schema: "meeting");

            migrationBuilder.DropTable(
                name: "staff_schedules",
                schema: "meeting");
        }
    }
}
