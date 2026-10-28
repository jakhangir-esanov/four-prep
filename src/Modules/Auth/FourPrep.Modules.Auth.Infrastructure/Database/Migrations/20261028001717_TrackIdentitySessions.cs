using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Auth.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class TrackIdentitySessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_session_id",
                schema: "auth",
                table: "user_sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "refreshed_on_utc",
                schema: "auth",
                table: "user_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_identity_session_id",
                schema: "auth",
                table: "user_sessions",
                column: "identity_session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_sessions_identity_session_id",
                schema: "auth",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "identity_session_id",
                schema: "auth",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "refreshed_on_utc",
                schema: "auth",
                table: "user_sessions");
        }
    }
}
