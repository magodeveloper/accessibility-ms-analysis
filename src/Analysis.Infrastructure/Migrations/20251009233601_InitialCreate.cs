using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analysis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "analysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    date_analysis = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    content_type = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_input = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tool_used = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    summary_result = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    result_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    wcag_version = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duration_ms = table.Column<int>(type: "int", nullable: true),
                    wcag_level = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    axe_violations = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    axe_needs_review = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    axe_recommendations = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    axe_passes = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    axe_incomplete = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    axe_inapplicable = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ea_violations = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ea_needs_review = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ea_recommendations = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ea_passes = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ea_incomplete = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ea_inapplicable = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    analysis_id = table.Column<int>(type: "int", nullable: false),
                    wcag_criterion_id = table.Column<int>(type: "int", nullable: false),
                    wcag_criterion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    level = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    severity = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_results_analysis_analysis_id",
                        column: x => x.analysis_id,
                        principalTable: "analysis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "errors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    result_id = table.Column<int>(type: "int", nullable: false),
                    wcag_criterion_id = table.Column<int>(type: "int", nullable: false),
                    error_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_errors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_errors_results_result_id",
                        column: x => x.result_id,
                        principalTable: "results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_analysis_date",
                table: "analysis",
                column: "date_analysis");

            migrationBuilder.CreateIndex(
                name: "idx_analysis_status",
                table: "analysis",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_analysis_user",
                table: "analysis",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_errors_result",
                table: "errors",
                column: "result_id");

            migrationBuilder.CreateIndex(
                name: "idx_results_analysis",
                table: "results",
                column: "analysis_id");

            migrationBuilder.CreateIndex(
                name: "idx_results_severity",
                table: "results",
                column: "severity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "errors");

            migrationBuilder.DropTable(
                name: "results");

            migrationBuilder.DropTable(
                name: "analysis");
        }
    }
}
