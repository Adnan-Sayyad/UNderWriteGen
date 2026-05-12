using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributionAndPartyManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIdToCustomerParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "CustomerParty",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CustomerParty");
        }
    }
}
