using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationsAndAlerts.Migrations
{
    /// <inheritdoc />
    public partial class emailIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new SenderEmail column (required, default to 'system@uwpro.internal'
            // so existing rows are valid).
            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "Notification",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "system@uwpro.internal");

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
