using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FourPrep.Modules.Catalogue.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalogue");

            migrationBuilder.CreateTable(
                name: "destinations",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    visa_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_destinations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "universities",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ranking_source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ranking = table.Column<int>(type: "integer", nullable: true),
                    acceptance_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    is_partner = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "jsonb", nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_on = table.Column<DateOnly>(type: "date", nullable: false),
                    checked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_universities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "costs",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    tuition_minor = table.Column<long>(type: "bigint", nullable: false),
                    living_minor = table.Column<long>(type: "bigint", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_on = table.Column<DateOnly>(type: "date", nullable: false),
                    university_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_costs", x => x.id);
                    table.ForeignKey(
                        name: "fk_costs_universities_university_id",
                        column: x => x.university_id,
                        principalSchema: "catalogue",
                        principalTable: "universities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "programs",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    university_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    degree_level = table.Column<int>(type: "integer", nullable: false),
                    field = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    duration_months = table.Column<int>(type: "integer", nullable: true),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_on = table.Column<DateOnly>(type: "date", nullable: false),
                    checked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_on_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_programs", x => x.id);
                    table.ForeignKey(
                        name: "fk_programs_universities_university_id",
                        column: x => x.university_id,
                        principalSchema: "catalogue",
                        principalTable: "universities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scholarships",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<string>(type: "jsonb", nullable: false),
                    eligibility = table.Column<string>(type: "jsonb", nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_on = table.Column<DateOnly>(type: "date", nullable: false),
                    university_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scholarships", x => x.id);
                    table.ForeignKey(
                        name: "fk_scholarships_universities_university_id",
                        column: x => x.university_id,
                        principalSchema: "catalogue",
                        principalTable: "universities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admission_requirements",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    intake_term = table.Column<int>(type: "integer", nullable: false),
                    intake_year = table.Column<int>(type: "integer", nullable: false),
                    english_minimums = table.Column<string>(type: "jsonb", nullable: false),
                    sat_policy = table.Column<int>(type: "integer", nullable: true),
                    sat_mid_range_low = table.Column<int>(type: "integer", nullable: true),
                    sat_mid_range_high = table.Column<int>(type: "integer", nullable: true),
                    minimum_gpa = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    gpa_scale = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    documents = table.Column<List<string>>(type: "text[]", nullable: false),
                    essays_count = table.Column<int>(type: "integer", nullable: true),
                    interview_required = table.Column<bool>(type: "boolean", nullable: true),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_on = table.Column<DateOnly>(type: "date", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admission_requirements", x => x.id);
                    table.ForeignKey(
                        name: "fk_admission_requirements_programs_program_id",
                        column: x => x.program_id,
                        principalSchema: "catalogue",
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deadlines",
                schema: "catalogue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_on = table.Column<DateOnly>(type: "date", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deadlines", x => x.id);
                    table.ForeignKey(
                        name: "fk_deadlines_programs_program_id",
                        column: x => x.program_id,
                        principalSchema: "catalogue",
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admission_requirements_program_id",
                schema: "catalogue",
                table: "admission_requirements",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "ix_costs_university_id",
                schema: "catalogue",
                table: "costs",
                column: "university_id");

            migrationBuilder.CreateIndex(
                name: "ix_deadlines_program_id",
                schema: "catalogue",
                table: "deadlines",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "ix_destinations_country_code",
                schema: "catalogue",
                table: "destinations",
                column: "country_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_programs_degree_level",
                schema: "catalogue",
                table: "programs",
                column: "degree_level");

            migrationBuilder.CreateIndex(
                name: "ix_programs_field",
                schema: "catalogue",
                table: "programs",
                column: "field");

            migrationBuilder.CreateIndex(
                name: "ix_programs_university_id",
                schema: "catalogue",
                table: "programs",
                column: "university_id");

            migrationBuilder.CreateIndex(
                name: "ix_scholarships_university_id",
                schema: "catalogue",
                table: "scholarships",
                column: "university_id");

            migrationBuilder.CreateIndex(
                name: "ix_universities_country_code",
                schema: "catalogue",
                table: "universities",
                column: "country_code");

            migrationBuilder.CreateIndex(
                name: "ix_universities_slug",
                schema: "catalogue",
                table: "universities",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admission_requirements",
                schema: "catalogue");

            migrationBuilder.DropTable(
                name: "costs",
                schema: "catalogue");

            migrationBuilder.DropTable(
                name: "deadlines",
                schema: "catalogue");

            migrationBuilder.DropTable(
                name: "destinations",
                schema: "catalogue");

            migrationBuilder.DropTable(
                name: "scholarships",
                schema: "catalogue");

            migrationBuilder.DropTable(
                name: "programs",
                schema: "catalogue");

            migrationBuilder.DropTable(
                name: "universities",
                schema: "catalogue");
        }
    }
}
