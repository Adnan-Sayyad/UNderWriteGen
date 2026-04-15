using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DistributionAndPartyManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agent",
                columns: table => new
                {
                    AgentID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProducerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agent", x => x.AgentID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerParty",
                columns: table => new
                {
                    PartyID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DOBIncorporation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContactInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Segment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerParty", x => x.PartyID);
                });

            migrationBuilder.InsertData(
                table: "Agent",
                columns: new[] { "AgentID", "ContactInfo", "Name", "ProducerCode", "Region", "Status" },
                values: new object[,]
                {
                    { "AGT-20250101-0001", "Mumbai Office, Andheri West", "Rajesh Kumar", "MUM-LIF-001", "Mumbai", "Active" },
                    { "AGT-20250101-0002", "Delhi Office, Connaught Place", "Priya Sharma", "DEL-PNC-002", "Delhi", "Active" }
                });

            migrationBuilder.InsertData(
                table: "CustomerParty",
                columns: new[] { "PartyID", "ContactInfo", "DOBIncorporation", "Name", "PartyType", "Segment", "Status" },
                values: new object[,]
                {
                    { "PTY-20250101-0001", "Pune, Maharashtra", new DateTime(1985, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amit Patel", "Individual", "Retail", "Active" },
                    { "PTY-20250101-0002", "Bangalore, Karnataka", new DateTime(2010, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "TechCorp Solutions Pvt Ltd", "Organization", "SME", "Active" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agent_Name",
                table: "Agent",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Agent_ProducerCode",
                table: "Agent",
                column: "ProducerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agent_Region",
                table: "Agent",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Agent_Status",
                table: "Agent",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerParty_Name",
                table: "CustomerParty",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerParty_PartyType",
                table: "CustomerParty",
                column: "PartyType");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerParty_Segment",
                table: "CustomerParty",
                column: "Segment");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerParty_Status",
                table: "CustomerParty",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agent");

            migrationBuilder.DropTable(
                name: "CustomerParty");
        }
    }
}
