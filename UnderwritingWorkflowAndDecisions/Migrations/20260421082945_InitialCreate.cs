using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnderwritingWorkflowAndDecisions.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subjectivities",
                columns: table => new
                {
                    SubjectivityID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjectivities", x => x.SubjectivityID);
                });

            migrationBuilder.CreateTable(
                name: "UWDecisions",
                columns: table => new
                {
                    DecisionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecidedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UWDecisions", x => x.DecisionID);
                });

            migrationBuilder.CreateTable(
                name: "UWNotes",
                columns: table => new
                {
                    NoteID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UWNotes", x => x.NoteID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subjectivities");

            migrationBuilder.DropTable(
                name: "UWDecisions");

            migrationBuilder.DropTable(
                name: "UWNotes");
        }
    }
}
