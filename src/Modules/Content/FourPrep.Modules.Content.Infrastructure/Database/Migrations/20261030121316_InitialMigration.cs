using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Content.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "news_posts",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    title = table.Column<string>(type: "jsonb", nullable: false),
                    body = table.Column<string>(type: "jsonb", nullable: false),
                    tag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cover_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cover_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    published_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news_posts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_news_posts_cover_file_id",
                schema: "content",
                table: "news_posts",
                column: "cover_file_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_news_posts_slug",
                schema: "content",
                table: "news_posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_news_posts_status_published_on_utc",
                schema: "content",
                table: "news_posts",
                columns: new[] { "status", "published_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_news_posts_tag",
                schema: "content",
                table: "news_posts",
                column: "tag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_posts",
                schema: "content");
        }
    }
}
