using ComplianceAuditAndQA.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAuditAndQA.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ComplianceDbContext context)
        {
            Console.WriteLine("[Seeder] Checking database connection…");
            await context.Database.MigrateAsync();
            Console.WriteLine("[Seeder] Migration applied (or already up to date).");

            var seeded = false;

            // ── Compliance Checklists ─────────────────────────────────────
            if (!await context.ComplianceChecklists.AnyAsync())
            {
                Console.WriteLine("[Seeder] Seeding ComplianceChecklists…");

                // Shared submission IDs (reused across all 3 tables)
                var sub1 = new Guid("A1B2C3D4-0001-0001-0001-000000000001");
                var sub2 = new Guid("A1B2C3D4-0002-0002-0002-000000000002");
                var sub3 = new Guid("A1B2C3D4-0003-0003-0003-000000000003");
                var sub4 = new Guid("A1B2C3D4-0004-0004-0004-000000000004");
                var sub5 = new Guid("A1B2C3D4-0005-0005-0005-000000000005");

                var checklists = new List<ComplianceChecklist>
                {
                    new()
                    {
                        ChecklistId   = new Guid("C0000001-0000-0000-0000-000000000001"),
                        SubmissionId  = sub1,
                        ItemsJson     = "[{\"item\":\"KYC documents verified\",\"checked\":true},{\"item\":\"Risk assessment completed\",\"checked\":true},{\"item\":\"Premium calculation reviewed\",\"checked\":true},{\"item\":\"Policy wording approved\",\"checked\":true}]",
                        CompletedBy   = "priya.sharma@underwritepro.com",
                        CompletedDate = new DateTime(2026, 4, 10, 9, 30, 0, DateTimeKind.Utc),
                        Status        = "Completed",
                        CreatedAt     = new DateTime(2026, 4, 8, 8, 0, 0, DateTimeKind.Utc),
                        UpdatedAt     = new DateTime(2026, 4, 10, 9, 30, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ChecklistId   = new Guid("C0000002-0000-0000-0000-000000000002"),
                        SubmissionId  = sub2,
                        ItemsJson     = "[{\"item\":\"KYC documents verified\",\"checked\":true},{\"item\":\"Risk assessment completed\",\"checked\":true},{\"item\":\"Premium calculation reviewed\",\"checked\":false},{\"item\":\"Policy wording approved\",\"checked\":false}]",
                        CompletedBy   = "rahul.mehta@underwritepro.com",
                        CompletedDate = null,
                        Status        = "InProgress",
                        CreatedAt     = new DateTime(2026, 4, 12, 10, 15, 0, DateTimeKind.Utc),
                        UpdatedAt     = new DateTime(2026, 4, 13, 14, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ChecklistId   = new Guid("C0000003-0000-0000-0000-000000000003"),
                        SubmissionId  = sub3,
                        ItemsJson     = "[{\"item\":\"KYC documents verified\",\"checked\":false},{\"item\":\"Risk assessment completed\",\"checked\":false},{\"item\":\"Premium calculation reviewed\",\"checked\":false},{\"item\":\"Sanctions screening done\",\"checked\":false}]",
                        CompletedBy   = null,
                        CompletedDate = null,
                        Status        = "Pending",
                        CreatedAt     = new DateTime(2026, 4, 15, 11, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ChecklistId   = new Guid("C0000004-0000-0000-0000-000000000004"),
                        SubmissionId  = sub4,
                        ItemsJson     = "[{\"item\":\"KYC documents verified\",\"checked\":true},{\"item\":\"Risk assessment completed\",\"checked\":true},{\"item\":\"Claims history checked\",\"checked\":true},{\"item\":\"Reinsurance limits confirmed\",\"checked\":true},{\"item\":\"Policy wording approved\",\"checked\":true}]",
                        CompletedBy   = "anita.joshi@underwritepro.com",
                        CompletedDate = new DateTime(2026, 4, 20, 16, 45, 0, DateTimeKind.Utc),
                        Status        = "Completed",
                        CreatedAt     = new DateTime(2026, 4, 18, 9, 0, 0, DateTimeKind.Utc),
                        UpdatedAt     = new DateTime(2026, 4, 20, 16, 45, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ChecklistId   = new Guid("C0000005-0000-0000-0000-000000000005"),
                        SubmissionId  = sub5,
                        ItemsJson     = "[{\"item\":\"KYC documents verified\",\"checked\":false},{\"item\":\"Risk survey scheduled\",\"checked\":false},{\"item\":\"Premium calculation reviewed\",\"checked\":false}]",
                        CompletedBy   = null,
                        CompletedDate = null,
                        Status        = "Pending",
                        CreatedAt     = new DateTime(2026, 5, 1, 8, 30, 0, DateTimeKind.Utc),
                    },
                };

                await context.ComplianceChecklists.AddRangeAsync(checklists);
                Console.WriteLine($"[Seeder] {checklists.Count} checklists queued.");
                seeded = true;
            }
            else
            {
                Console.WriteLine("[Seeder] ComplianceChecklists already has data — skipped.");
            }

            // ── Authority Breaches ────────────────────────────────────────
            if (!await context.AuthorityBreaches.AnyAsync())
            {
                Console.WriteLine("[Seeder] Seeding AuthorityBreaches…");

                var sub1 = new Guid("A1B2C3D4-0001-0001-0001-000000000001");
                var sub2 = new Guid("A1B2C3D4-0002-0002-0002-000000000002");
                var sub3 = new Guid("A1B2C3D4-0003-0003-0003-000000000003");
                var sub4 = new Guid("A1B2C3D4-0004-0004-0004-000000000004");
                var sub5 = new Guid("A1B2C3D4-0005-0005-0005-000000000005");

                var breaches = new List<AuthorityBreach>
                {
                    new()
                    {
                        BreachId     = new Guid("B0000001-0000-0000-0000-000000000001"),
                        SubmissionId = sub1,
                        BreachType   = "Authority",
                        Description  = "Underwriter approved a policy limit of Rs 2.5 Cr against their individual authority limit of Rs 1.5 Cr. Escalated and approved by Senior UW.",
                        ApprovedBy   = "vikram.nair@underwritepro.com",
                        ApprovedDate = new DateTime(2026, 4, 9, 11, 0, 0, DateTimeKind.Utc),
                        Status       = "Approved",
                        CreatedAt    = new DateTime(2026, 4, 8, 15, 0, 0, DateTimeKind.Utc),
                        UpdatedAt    = new DateTime(2026, 4, 9, 11, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        BreachId     = new Guid("B0000002-0000-0000-0000-000000000002"),
                        SubmissionId = sub2,
                        BreachType   = "PricingTolerance",
                        Description  = "Quoted premium is 18% below the minimum technical rate for this risk class. Deviation exceeds the allowed 10% tolerance band.",
                        ApprovedBy   = "vikram.nair@underwritepro.com",
                        ApprovedDate = new DateTime(2026, 4, 14, 9, 30, 0, DateTimeKind.Utc),
                        Status       = "Rejected",
                        CreatedAt    = new DateTime(2026, 4, 13, 16, 0, 0, DateTimeKind.Utc),
                        UpdatedAt    = new DateTime(2026, 4, 14, 9, 30, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        BreachId     = new Guid("B0000003-0000-0000-0000-000000000003"),
                        SubmissionId = sub3,
                        BreachType   = "RuleOverride",
                        Description  = "Request to waive mandatory risk inspection for a commercial property policy above Rs 50L sum insured. Property located in a high flood-risk zone.",
                        ApprovedBy   = null,
                        ApprovedDate = null,
                        Status       = "Pending",
                        CreatedAt    = new DateTime(2026, 4, 16, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        BreachId     = new Guid("B0000004-0000-0000-0000-000000000004"),
                        SubmissionId = sub4,
                        BreachType   = "Authority",
                        Description  = "Motor fleet policy for 120 vehicles processed by junior underwriter whose authority is limited to 50 vehicles. Referred to branch head.",
                        ApprovedBy   = "deepa.iyer@underwritepro.com",
                        ApprovedDate = new DateTime(2026, 4, 19, 14, 0, 0, DateTimeKind.Utc),
                        Status       = "Approved",
                        CreatedAt    = new DateTime(2026, 4, 18, 12, 0, 0, DateTimeKind.Utc),
                        UpdatedAt    = new DateTime(2026, 4, 19, 14, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        BreachId     = new Guid("B0000005-0000-0000-0000-000000000005"),
                        SubmissionId = sub5,
                        BreachType   = "PricingTolerance",
                        Description  = "Marine cargo policy premium quoted at Rs 12000 against system-calculated minimum of Rs 15500. Broker requesting competitive pricing for renewal retention.",
                        ApprovedBy   = null,
                        ApprovedDate = null,
                        Status       = "Pending",
                        CreatedAt    = new DateTime(2026, 5, 2, 9, 0, 0, DateTimeKind.Utc),
                    },
                };

                await context.AuthorityBreaches.AddRangeAsync(breaches);
                Console.WriteLine($"[Seeder] {breaches.Count} authority breaches queued.");
                seeded = true;
            }
            else
            {
                Console.WriteLine("[Seeder] AuthorityBreaches already has data — skipped.");
            }

            // ── Exception Logs ────────────────────────────────────────────
            if (!await context.ExceptionLogs.AnyAsync())
            {
                Console.WriteLine("[Seeder] Seeding ExceptionLogs…");

                var sub1 = new Guid("A1B2C3D4-0001-0001-0001-000000000001");
                var sub2 = new Guid("A1B2C3D4-0002-0002-0002-000000000002");
                var sub3 = new Guid("A1B2C3D4-0003-0003-0003-000000000003");
                var sub4 = new Guid("A1B2C3D4-0004-0004-0004-000000000004");
                var sub5 = new Guid("A1B2C3D4-0005-0005-0005-000000000005");

                var exceptions = new List<ExceptionLog>
                {
                    new()
                    {
                        ExceptionId  = new Guid("E0000001-0000-0000-0000-000000000001"),
                        SubmissionId = sub1,
                        Category     = "Data",
                        Details      = "Insured PAN number could not be validated against NSDL database. Manual verification completed offline and document copy retained on file.",
                        LoggedDate   = new DateTime(2026, 4, 8, 10, 0, 0, DateTimeKind.Utc),
                        Status       = "Closed",
                        CreatedAt    = new DateTime(2026, 4, 8, 10, 0, 0, DateTimeKind.Utc),
                        UpdatedAt    = new DateTime(2026, 4, 9, 9, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ExceptionId  = new Guid("E0000002-0000-0000-0000-000000000002"),
                        SubmissionId = sub2,
                        Category     = "Compliance",
                        Details      = "AML screening flagged the insured entity as a politically exposed person (PEP). Enhanced due diligence documentation pending from the agent.",
                        LoggedDate   = new DateTime(2026, 4, 13, 11, 30, 0, DateTimeKind.Utc),
                        Status       = "Open",
                        CreatedAt    = new DateTime(2026, 4, 13, 11, 30, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ExceptionId  = new Guid("E0000003-0000-0000-0000-000000000003"),
                        SubmissionId = sub3,
                        Category     = "Process",
                        Details      = "Policy issuance SLA breached — submission was pending underwriter review for 9 business days against the mandated 5-day TAT. Delay attributed to missing risk survey report.",
                        LoggedDate   = new DateTime(2026, 4, 17, 14, 0, 0, DateTimeKind.Utc),
                        Status       = "Open",
                        CreatedAt    = new DateTime(2026, 4, 17, 14, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ExceptionId  = new Guid("E0000004-0000-0000-0000-000000000004"),
                        SubmissionId = sub4,
                        Category     = "Process",
                        Details      = "Endorsement request processed without supervisor countersignature as required by SOP for changes above Rs 10000 additional premium. Retrospective sign-off obtained.",
                        LoggedDate   = new DateTime(2026, 4, 19, 10, 0, 0, DateTimeKind.Utc),
                        Status       = "Closed",
                        CreatedAt    = new DateTime(2026, 4, 19, 10, 0, 0, DateTimeKind.Utc),
                        UpdatedAt    = new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ExceptionId  = new Guid("E0000005-0000-0000-0000-000000000005"),
                        SubmissionId = sub5,
                        Category     = "Data",
                        Details      = "GST number provided by the insured does not match the registered trade name on the GSTIN portal. Agent has been notified to provide a corrected certificate.",
                        LoggedDate   = new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc),
                        Status       = "Open",
                        CreatedAt    = new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc),
                    },
                    new()
                    {
                        ExceptionId  = new Guid("E0000006-0000-0000-0000-000000000006"),
                        SubmissionId = sub2,
                        Category     = "Data",
                        Details      = "Vehicle chassis number in the motor policy submission does not match the RC book uploaded. Discrepancy flagged for agent correction before bind.",
                        LoggedDate   = new DateTime(2026, 4, 14, 9, 0, 0, DateTimeKind.Utc),
                        Status       = "Open",
                        CreatedAt    = new DateTime(2026, 4, 14, 9, 0, 0, DateTimeKind.Utc),
                    },
                };

                await context.ExceptionLogs.AddRangeAsync(exceptions);
                Console.WriteLine($"[Seeder] {exceptions.Count} exception logs queued.");
                seeded = true;
            }
            else
            {
                Console.WriteLine("[Seeder] ExceptionLogs already has data — skipped.");
            }

            // ── Save all ──────────────────────────────────────────────────
            if (seeded)
            {
                await context.SaveChangesAsync();
                Console.WriteLine("[Seeder] ✅ Seed data saved successfully.");
            }
            else
            {
                Console.WriteLine("[Seeder] ✅ All tables already contain data. Nothing to seed.");
            }
        }
    }
}
