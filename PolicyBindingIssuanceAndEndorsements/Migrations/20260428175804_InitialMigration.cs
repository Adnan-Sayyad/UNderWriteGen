using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyBindingIssuanceAndEndorsements.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    PolicyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductLine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CoverageJSON = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    InceptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.PolicyID);
                });

            migrationBuilder.CreateTable(
                name: "Cancellations",
                columns: table => new
                {
                    CancellationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    PolicyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndorsementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangesJSON = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Renewals",
                columns: table => new
                {
                    RenewalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RenewalOfferJSON = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    OfferedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Offered")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Renewals", x => x.RenewalID);
                    table.ForeignKey(
                        name: "FK_Renewals_Policies_PolicyID",
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

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Renewals_PolicyID",
                table: "Renewals",
                column: "PolicyID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cancellations");

            migrationBuilder.DropTable(
                name: "Endorsements");

            migrationBuilder.DropTable(
                name: "Renewals");

            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
