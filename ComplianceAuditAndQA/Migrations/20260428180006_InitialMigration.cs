using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceAuditAndQA.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorityBreaches",
                columns: table => new
                {
                    BreachId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BreachType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityBreaches", x => x.BreachId);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceChecklists",
                columns: table => new
                {
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceChecklists", x => x.ChecklistId);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionLogs",
                columns: table => new
                {
                    ExceptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoggedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionLogs", x => x.ExceptionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityBreaches_BreachType",
                table: "AuthorityBreaches",
                column: "BreachType");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityBreaches_IsDeleted",
                table: "AuthorityBreaches",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityBreaches_Status",
                table: "AuthorityBreaches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityBreaches_SubmissionId",
                table: "AuthorityBreaches",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecklists_IsDeleted",
                table: "ComplianceChecklists",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecklists_Status",
                table: "ComplianceChecklists",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecklists_SubmissionId",
                table: "ComplianceChecklists",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLogs_Category",
                table: "ExceptionLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLogs_IsDeleted",
                table: "ExceptionLogs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLogs_Status",
                table: "ExceptionLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLogs_SubmissionId",
                table: "ExceptionLogs",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorityBreaches");

            migrationBuilder.DropTable(
                name: "ComplianceChecklists");

            migrationBuilder.DropTable(
                name: "ExceptionLogs");
        }
    }
}
