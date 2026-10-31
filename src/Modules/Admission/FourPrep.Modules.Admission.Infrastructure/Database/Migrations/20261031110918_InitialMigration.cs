using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Admission.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admission");

            migrationBuilder.CreateTable(
                name: "academy_applications",
                schema: "admission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    interests = table.Column<string>(type: "jsonb", nullable: false),
                    contact_channels = table.Column<string>(type: "jsonb", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    telegram_username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    meeting_language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    question = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    parent_consent_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    submitted_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    decline_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    withdrawn_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academy_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reported_scores",
                schema: "admission",
                columns: table => new
                {
                    exam = table.Column<int>(type: "integer", nullable: false),
                    academy_application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reported_scores", x => new { x.academy_application_id, x.exam });
                    table.ForeignKey(
                        name: "fk_reported_scores_academy_applications_academy_application_id",
                        column: x => x.academy_application_id,
                        principalSchema: "admission",
                        principalTable: "academy_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_academy_applications_status_submitted_on_utc",
                schema: "admission",
                table: "academy_applications",
                columns: new[] { "status", "submitted_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_academy_applications_student_user_id",
                schema: "admission",
                table: "academy_applications",
                column: "student_user_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ix_academy_applications_student_user_id_submitted_on_utc",
                schema: "admission",
                table: "academy_applications",
                columns: new[] { "student_user_id", "submitted_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reported_scores",
                schema: "admission");

            migrationBuilder.DropTable(
                name: "academy_applications",
                schema: "admission");
        }
    }
}
