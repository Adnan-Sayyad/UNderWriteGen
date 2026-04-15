using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportingAndPortfolioAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialM1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UWReports",
                columns: table => new
                {
                    ReportID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScopeValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Metrics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UWReports", x => x.ReportID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UWReports_GeneratedDate",
                table: "UWReports",
                column: "GeneratedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UWReports_Scope",
                table: "UWReports",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_UWReports_Scope_ScopeValue",
                table: "UWReports",
                columns: new[] { "Scope", "ScopeValue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UWReports");
        }
    }
}
