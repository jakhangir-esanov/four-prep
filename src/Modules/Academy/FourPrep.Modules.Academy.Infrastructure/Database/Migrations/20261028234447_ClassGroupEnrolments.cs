using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Academy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ClassGroupEnrolments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "academy",
                table: "class_groups",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "enrolments",
                schema: "academy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrolled_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    drop_reason = table.Column<int>(type: "integer", nullable: true),
                    payment_due_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    class_group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrolments", x => x.id);
                    table.ForeignKey(
                        name: "fk_enrolments_class_groups_class_group_id",
                        column: x => x.class_group_id,
                        principalSchema: "academy",
                        principalTable: "class_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_enrolments_class_group_id_status",
                schema: "academy",
                table: "enrolments",
                columns: new[] { "class_group_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_enrolments_status_payment_due_on_utc",
                schema: "academy",
                table: "enrolments",
                columns: new[] { "status", "payment_due_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_enrolments_student_user_id",
                schema: "academy",
                table: "enrolments",
                column: "student_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrolments",
                schema: "academy");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "academy",
                table: "class_groups");
        }
    }
}
