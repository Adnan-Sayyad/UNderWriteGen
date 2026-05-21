using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubmissionAndIntake.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Questionnaires",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Questionnaires");
        }
    }
}
