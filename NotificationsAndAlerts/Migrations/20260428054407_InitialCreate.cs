using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationsAndAlerts.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationID);
                });

            migrationBuilder.InsertData(
                table: "Notification",
                columns: new[] { "NotificationID", "Category", "CreatedDate", "Message", "Status", "UserID" },
                values: new object[,]
                {
                    { "NTF-20260102-0001", "Referral", new DateTime(2026, 1, 2, 9, 30, 0, 0, DateTimeKind.Unspecified), "Submission SUB-2026-0042 has been referred for your review.", "Unread", "USR-001" },
                    { "NTF-20260103-0001", "Quote", new DateTime(2026, 1, 3, 11, 15, 0, 0, DateTimeKind.Unspecified), "Quote QT-2026-0099 has been generated and is awaiting customer acceptance.", "Read", "USR-002" },
                    { "NTF-20260104-0001", "SLA", new DateTime(2026, 1, 4, 8, 0, 0, 0, DateTimeKind.Unspecified), "SLA breach warning: case CASE-2026-0007 nearing 24-hour deadline.", "Unread", "USR-001" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Category",
                table: "Notification",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Status",
                table: "Notification",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserID",
                table: "Notification",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
