using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PricingQuotationAndTerms.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PricingParams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductLine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingParams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    BasePremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoadingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    DiscountsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    TaxesJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    TermsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PricingParams",
                columns: new[] { "Id", "CreatedAt", "Description", "EffectiveFrom", "EffectiveTo", "IsActive", "ParamName", "ProductLine", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("0b9a7156-3edd-7f3c-b429-f763fb7a6c10"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Agriculture: +15%", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_Agriculture", "Global", null, 0.15m },
                    { new Guid("0c56ca4e-50bd-5715-b3f7-f639fdaf4f7c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Property: 1.5% per annum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BaseRate", "Property", null, 0.015m },
                    { new Guid("0f14bed1-46ea-bccb-b729-2d6df8e8e9a7"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "+10% for Medium risk band", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "RiskLoading_Medium", "Global", null, 0.10m },
                    { new Guid("19c49211-ed49-5d4e-a831-0b2cc1bcf955"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "3% discount for 12-month policy", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "TenureDiscount_12m", "Global", null, 0.03m },
                    { new Guid("21ade2e3-912c-1ff9-c4bb-9935a3cef11e"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mining: +30%", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_Mining", "Global", null, 0.30m },
                    { new Guid("273bd8e9-825d-152d-6c54-3a9edf33babe"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Construction: +20%", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_Construction", "Global", null, 0.20m },
                    { new Guid("7033ad09-0b4c-228b-f278-441dc87d8ec5"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Banking: 0% (low risk)", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_Banking", "Global", null, 0.00m },
                    { new Guid("7f259398-91b6-2a01-9f9b-32bac5ae13c1"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5% discount for 24-month policy", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "TenureDiscount_24m", "Global", null, 0.05m },
                    { new Guid("a8dd49ed-1842-99e3-bb44-2f323677faeb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Motor: 2.5% per annum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BaseRate", "Motor", null, 0.025m },
                    { new Guid("aba95d6c-9030-c97c-f52c-0083177e5bcb"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "+25% for High risk band", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "RiskLoading_High", "Global", null, 0.25m },
                    { new Guid("aea3272e-e239-0285-f6fd-b9ef4b8ae1ca"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Life: 2.0% per annum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BaseRate", "Life", null, 0.020m },
                    { new Guid("b5f7e7be-a834-5174-45b8-4661b17e74a9"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5% loyalty discount for renewals", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "LoyaltyDiscount", "Global", null, 0.05m },
                    { new Guid("b6304b58-5706-c9dc-e83a-e9d43f452e25"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "7% discount for 36-month policy", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "TenureDiscount_36m", "Global", null, 0.07m },
                    { new Guid("cdccbf79-4f65-d06e-8f9a-a86cf5de5027"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Education: 0% (low risk)", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_Education", "Global", null, 0.00m },
                    { new Guid("d3488aef-ae37-f866-2734-2ca22c5b4df4"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "IT: 0% (low risk)", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_IT", "Global", null, 0.00m },
                    { new Guid("d94b7302-bd47-1880-8f91-b7f519ec212c"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GST: 18% on adjusted premium", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "GstRate", "Global", null, 0.18m },
                    { new Guid("e325ee97-1b28-df21-4af9-97acbce5c115"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2% discount via preferred agent", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "AgentDiscount_Preferred", "Global", null, 0.02m },
                    { new Guid("ea9c8223-a222-d5b9-6c28-e61b447a4ead"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transport: +12%", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "OccupationLoad_Transport", "Global", null, 0.12m },
                    { new Guid("f70ba0e4-1bb2-d177-bd69-5cc2b210f368"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Floor premium: ₹500", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "MinimumPremium", "Global", null, 500m },
                    { new Guid("fe0f55aa-0263-e1e5-eb4c-55bc0e8f5360"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Health: 3.5% per annum", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BaseRate", "Health", null, 0.035m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingParams_ProductLine_ParamName_Active",
                table: "PricingParams",
                columns: new[] { "ProductLine", "ParamName", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Status",
                table: "Quotes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_SubmissionId",
                table: "Quotes",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingParams");

            migrationBuilder.DropTable(
                name: "Quotes");
        }
    }
}
