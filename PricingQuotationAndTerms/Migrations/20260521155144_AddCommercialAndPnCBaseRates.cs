using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PricingQuotationAndTerms.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialAndPnCBaseRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PricingParams",
                columns: new[] { "Id", "CreatedAt", "Description", "EffectiveFrom", "EffectiveTo", "IsActive", "ParamName", "ProductLine", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("948c0211-295d-c93d-8a39-21f537cd6a50"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PnC floor premium: ₹750", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "MinimumPremium", "PnC", null, 750m },
                    { new Guid("efb4a3c9-6f1f-c8ea-9dbf-28be4adeb286"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PnC: 2.2% per annum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BaseRate", "PnC", null, 0.022m },
                    { new Guid("f1d88bb2-77c0-8eb3-c589-2afed8bc33bd"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Commercial: 1.8% per annum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BaseRate", "Commercial", null, 0.018m },
                    { new Guid("ff347dfb-048a-72fa-0807-1ab7a01bf3c8"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Commercial floor premium: ₹1500", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "MinimumPremium", "Commercial", null, 1500m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PricingParams",
                keyColumn: "Id",
                keyValue: new Guid("948c0211-295d-c93d-8a39-21f537cd6a50"));

            migrationBuilder.DeleteData(
                table: "PricingParams",
                keyColumn: "Id",
                keyValue: new Guid("efb4a3c9-6f1f-c8ea-9dbf-28be4adeb286"));

            migrationBuilder.DeleteData(
                table: "PricingParams",
                keyColumn: "Id",
                keyValue: new Guid("f1d88bb2-77c0-8eb3-c589-2afed8bc33bd"));

            migrationBuilder.DeleteData(
                table: "PricingParams",
                keyColumn: "Id",
                keyValue: new Guid("ff347dfb-048a-72fa-0807-1ab7a01bf3c8"));
        }
    }
}
