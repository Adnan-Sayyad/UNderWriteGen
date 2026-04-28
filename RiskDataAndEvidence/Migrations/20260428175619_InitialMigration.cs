using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskDataAndEvidence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidenceRefs",
                columns: table => new
                {
                    EvidenceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResultJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceRefs", x => x.EvidenceID);
                });

            migrationBuilder.CreateTable(
                name: "RiskProfiles",
                columns: table => new
                {
                    RiskID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttributesJSON = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskProfiles", x => x.RiskID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRefs_SubmissionID",
                table: "EvidenceRefs",
                column: "SubmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRefs_SubmissionID_EvidenceType",
                table: "EvidenceRefs",
                columns: new[] { "SubmissionID", "EvidenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskProfiles_RiskType",
                table: "RiskProfiles",
                column: "RiskType");

            migrationBuilder.CreateIndex(
                name: "IX_RiskProfiles_SubmissionID",
                table: "RiskProfiles",
                column: "SubmissionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceRefs");

            migrationBuilder.DropTable(
                name: "RiskProfiles");
        }
    }
}
