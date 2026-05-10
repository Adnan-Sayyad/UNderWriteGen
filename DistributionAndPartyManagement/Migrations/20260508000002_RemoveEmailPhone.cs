using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DistributionAndPartyManagement.Data;

#nullable disable

namespace DistributionAndPartyManagement.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260508000002_RemoveEmailPhone")]
    public partial class RemoveEmailPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Email", table: "Agent");
            migrationBuilder.DropColumn(name: "Phone", table: "Agent");
            migrationBuilder.DropColumn(name: "Email", table: "CustomerParty");
            migrationBuilder.DropColumn(name: "Phone", table: "CustomerParty");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "Email", table: "Agent",
                type: "nvarchar(254)", maxLength: 254, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Phone", table: "Agent",
                type: "nvarchar(15)", maxLength: 15, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Email", table: "CustomerParty",
                type: "nvarchar(254)", maxLength: 254, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Phone", table: "CustomerParty",
                type: "nvarchar(15)", maxLength: 15, nullable: true);
        }
    }
}
