using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Meeting.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class BookerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_meetings_booked_by_user_id_status",
                schema: "meeting",
                table: "meetings",
                columns: new[] { "booked_by_user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_meetings_booked_by_user_id_status",
                schema: "meeting",
                table: "meetings");
        }
    }
}
