using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesScoringAndReferralMatrix.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReferralMatrices",
                columns: table => new
                {
                    ReferralMatrixID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CriteriaJSON = table.Column<int>(type: "int", nullable: false),
                    RequiredAuthority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralMatrices", x => x.ReferralMatrixID);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    ReferralID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaisedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredAuthority = table.Column<int>(type: "int", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.ReferralID);
                });

            migrationBuilder.CreateTable(
                name: "RiskScores",
                columns: table => new
                {
                    RiskScoreID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoreValue = table.Column<double>(type: "float", nullable: false),
                    Band = table.Column<int>(type: "int", nullable: false),
                    ScoredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskScores", x => x.RiskScoreID);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    UWRuleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpressionJSON = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.UWRuleID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralMatrices");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "RiskScores");

            migrationBuilder.DropTable(
                name: "Rules");
        }
    }
}
