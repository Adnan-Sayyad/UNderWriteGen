using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationsAndAlerts.Migrations
{
    /// <inheritdoc />
    public partial class addSenderEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "Notification",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderEmail",
                table: "Notification",
                column: "SenderEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notification_SenderEmail",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "SenderEmail",
                table: "Notification");
        }
    }
}
