using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesScoringAndReferralMatrix.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendlyRuleAndMatrixFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleName",
                table: "Rules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "ReferralMatrices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Threshold",
                table: "ReferralMatrices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "RuleName",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "ReferralMatrices");

            migrationBuilder.DropColumn(
                name: "Threshold",
                table: "ReferralMatrices");
        }
    }
}
