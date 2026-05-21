using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyBindingIssuanceAndEndorsements.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEndorsementsAndCancellations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cancellations");

            migrationBuilder.DropTable(
                name: "Endorsements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cancellations",
                columns: table => new
                {
                    CancellationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PolicyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefundPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Requested")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cancellations", x => x.CancellationID);
                    table.ForeignKey(
                        name: "FK_Cancellations_Policies_PolicyID",
                        column: x => x.PolicyID,
                        principalTable: "Policies",
                        principalColumn: "PolicyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Endorsements",
                columns: table => new
                {
                    EndorsementID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangesJSON = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndorsementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PolicyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PremiumDelta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Proposed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endorsements", x => x.EndorsementID);
                    table.ForeignKey(
                        name: "FK_Endorsements_Policies_PolicyID",
                        column: x => x.PolicyID,
                        principalTable: "Policies",
                        principalColumn: "PolicyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_PolicyID",
                table: "Cancellations",
                column: "PolicyID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Endorsements_PolicyID",
                table: "Endorsements",
                column: "PolicyID");
        }
    }
}
