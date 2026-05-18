-- ============================================================
-- UnderwritePro — FULL FRESH DEMO DATA
-- Workflow: Submission → UW Review → Quote → Policy Bind
-- ============================================================

-- Fixed GUIDs (all valid hex):
-- SUB-001: A0000001-0001-0000-0000-000000000001  Life,       Issued
-- SUB-002: A0000002-0002-0000-0000-000000000002  Commercial, Quoted
-- SUB-003: A0000003-0003-0000-0000-000000000003  Health,     UnderReview
-- SUB-004: A0000004-0004-0000-0000-000000000004  Commercial, Draft
-- Quote-1: B0000001-0001-0000-0000-000000000001
-- Quote-2: B0000002-0002-0000-0000-000000000002
-- Policy:  C0000001-0001-0000-0000-000000000001

-- ============================================================
-- 1. DISTRIBUTION & PARTY MANAGEMENT
-- ============================================================
USE [DistributionAndPartyManagement];

DELETE FROM [CustomerParty];
DELETE FROM [Agent];

INSERT INTO [Agent] (AgentID, Name, ProducerCode, ContactInfo, Region, Status) VALUES
('AGT-001', 'Arjun Mehta',   'PROD-1001', 'arjun.mehta@broker.com | +91-98765-43210',   'South India', 'Active'),
('AGT-002', 'Kavitha Rajan', 'PROD-1002', 'kavitha.rajan@broker.com | +91-98765-11111', 'North India', 'Active'),
('AGT-003', 'Sameer Patel',  'PROD-1003', 'sameer.patel@broker.com | +91-90000-22222',  'West India',  'Active');

INSERT INTO [CustomerParty] (PartyID, PartyType, Name, DOBIncorporation, ContactInfo, Segment, Status, CreatedByUserId) VALUES
('PTY-001', 'Individual', 'Rajesh Kumar',       '1985-06-15', 'rajesh.kumar@email.com | Chennai',  'Retail',     'Active', 'AGT-001'),
('PTY-002', 'Corporate',  'TechNova Solutions', '2010-03-20', 'cfo@technova.com | Bangalore',       'Commercial', 'Active', 'AGT-001'),
('PTY-003', 'Individual', 'Priya Nair',         '1990-11-25', 'priya.nair@email.com | Kochi',       'Retail',     'Active', 'AGT-002'),
('PTY-004', 'Corporate',  'BuildRight Infra',   '2005-07-10', 'accounts@buildright.com | Mumbai',   'Commercial', 'Active', 'AGT-003');
GO

-- ============================================================
-- 2. SUBMISSION & INTAKE
-- ============================================================
USE [SubmissionAndIntake];

DELETE FROM [CompletenessChecks];
DELETE FROM [Attachments];
DELETE FROM [Questionnaires];
DELETE FROM [Submissions];

-- ProductLine: Life=0, Health=1, PnC=2, Commercial=3
-- Status: Draft=0, IntakeComplete=1, UnderReview=2, Quoted=3, Declined=4, Expired=5, Approved=6, PolicyBound=7, Issued=8

INSERT INTO [Submissions] (SubmissionID, PartyID, AgentID, ProductLine, CoverageJSON, InceptionDate, CreatedDate, Status) VALUES
('A0000001-0001-0000-0000-000000000001', 'PTY-001', 'AGT-001', 0,
 '{"sumInsured":2500000,"coverageType":"Term Life","policyTenure":20,"occupationType":"Salaried","smoker":false,"age":38}',
 '2026-06-01', '2026-05-10', 8),

('A0000002-0002-0000-0000-000000000002', 'PTY-002', 'AGT-001', 3,
 '{"sumInsured":15000000,"coverageType":"Commercial Property","propertyValue":12000000,"constructionType":"RCC","floodZone":1}',
 '2026-07-01', '2026-05-12', 3),

('A0000003-0003-0000-0000-000000000003', 'PTY-003', 'AGT-002', 1,
 '{"sumInsured":1000000,"coverageType":"Family Floater","members":4,"preExistingCondition":false,"bmi":24.5}',
 '2026-06-15', '2026-05-15', 2),

('A0000004-0004-0000-0000-000000000004', 'PTY-004', 'AGT-003', 3,
 '{"sumInsured":50000000,"coverageType":"Public Liability","annualTurnover":45000000,"industryCode":"CONST"}',
 '2026-07-15', '2026-05-17', 0);

INSERT INTO [Questionnaires] (QID, SubmissionID, TemplateVersion, ResponsesJSON, CompletedDate) VALUES
(NEWID(), 'A0000001-0001-0000-0000-000000000001', 'v2.1',
 '{"q1":"No prior claims","q2":"Non-smoker 10+ years","q3":"Annual health check done","q4":"No serious illness"}',
 '2026-05-11'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002', 'v3.0',
 '{"q1":"Property built 2018","q2":"Fire suppression installed","q3":"Security guards 24x7","q4":"No claims last 3 years"}',
 '2026-05-13');

INSERT INTO [CompletenessChecks] (CheckID, SubmissionID, MissingItemsJSON, Status, CheckedDate) VALUES
(NEWID(), 'A0000001-0001-0000-0000-000000000001', '[]',                                   1, '2026-05-11'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002', '[]',                                   1, '2026-05-13'),
(NEWID(), 'A0000003-0003-0000-0000-000000000003', '["Medical report pending"]',            0, '2026-05-15'),
(NEWID(), 'A0000004-0004-0000-0000-000000000004', '["Financial statements","GST cert"]',   0, NULL);
GO

-- ============================================================
-- 3. RULES SCORING & REFERRAL MATRIX
-- ============================================================
USE [RulesScoringAndReferralMatrix];

DELETE FROM [Referrals];
DELETE FROM [RiskScores];

-- Band: Low=0, Medium=1, High=2
INSERT INTO [RiskScores] (RiskScoreID, SubmissionID, ModelVersion, ScoreValue, Band, ScoredDate) VALUES
(NEWID(), 'A0000001-0001-0000-0000-000000000001', 'v2.0', 28.5, 0, '2026-05-11'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002', 'v2.0', 62.0, 1, '2026-05-13'),
(NEWID(), 'A0000003-0003-0000-0000-000000000003', 'v2.0', 45.5, 1, '2026-05-15');

-- RequiredAuthority: UW1=0, UW2=1, SeniorUW=2, Committee=3
-- Status: Pending=0, InReview=1, Resolved=2, Escalated=3
INSERT INTO [Referrals] (ReferralID, SubmissionID, RaisedBy, Reason, RequiredAuthority, AssignedTo, CreatedDate, Status) VALUES
(NEWID(), 'A0000002-0002-0000-0000-000000000002',
 'john.doe@underwritepro.com',
 'Commercial property sum insured 1.5 Cr exceeds standard UW1 authority. Senior sign-off required.',
 1, 'john.doe@underwritepro.com', '2026-05-13', 0);
GO

-- ============================================================
-- 4. PRICING, QUOTATION & TERMS
-- ============================================================
USE [PricingQuotationAndTerms];

DELETE FROM [Quotes];

INSERT INTO [Quotes] (Id, SubmissionId, VersionNo, BasePremium, TotalPremium, LoadingsJson, DiscountsJson, TaxesJson, TermsJson, ValidUntil, CreatedAt, AcceptedAt, Status) VALUES
('B0000001-0001-0000-0000-000000000001',
 'A0000001-0001-0000-0000-000000000001',
 1, 28750.00, 33925.00,
 '{"ageLoading":0,"smokerLoading":0,"occupationLoading":0}',
 '{"tenureDiscount":0.05,"loyaltyDiscount":0}',
 '{"gst":0.18}',
 '{"paymentFrequency":"Annual","gracePeriod":30,"claimSettlementRatio":"98%"}',
 '2026-06-30', '2026-05-12', '2026-05-14', 'Accepted'),

('B0000002-0002-0000-0000-000000000002',
 'A0000002-0002-0000-0000-000000000002',
 1, 185000.00, 213210.00,
 '{"propertyValueLoading":0.05,"floodZoneLoading":0}',
 '{"fireSystemDiscount":0.03,"preferredAgentDiscount":0.02}',
 '{"gst":0.18}',
 '{"paymentFrequency":"Quarterly","deductible":50000,"reinstatement":"Automatic"}',
 '2026-06-15', '2026-05-14', NULL, 'Presented');
GO

-- ============================================================
-- 5. UNDERWRITING WORKFLOW & DECISIONS
-- ============================================================
USE [UnderwritingWorkflowAndDecisions];

DELETE FROM [Subjectivities];
DELETE FROM [UWDecisions];
DELETE FROM [UWNotes];

INSERT INTO [UWNotes] (NoteID, SubmissionID, AuthorID, NoteText, CreatedDate) VALUES
(NEWID(), 'A0000001-0001-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001',
 'Rajesh Kumar, 38 yrs, non-smoker. Risk score 28.5 (Low). No pre-existing conditions. Medical reports clean. Approved.',
 '2026-05-11'),
(NEWID(), 'A0000001-0001-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001',
 'Quote Rs 33,925 annual. GST 18% included. Presented 12-May, accepted 14-May. Policy issued.',
 '2026-05-14'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001',
 'TechNova Solutions — 1.5 Cr commercial property. Referral raised for UW2 authority.',
 '2026-05-13'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001',
 'Senior UW approved. Fire suppression and 24x7 security are positive factors. Subjectivity on annual inspection.',
 '2026-05-14');

INSERT INTO [UWDecisions] (DecisionID, SubmissionID, Decision, Reason, DecidedBy, DecidedDate) VALUES
(NEWID(), 'A0000001-0001-0000-0000-000000000001',
 'Approve', 'Low risk profile. Clean medical history. Standard premium applied.',
 '00000000-0000-0000-0000-000000000001', '2026-05-11'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002',
 'Approve', 'Approved subject to annual property inspection within 30 days of inception.',
 '00000000-0000-0000-0000-000000000001', '2026-05-14'),
(NEWID(), 'A0000003-0003-0000-0000-000000000003',
 'MoreInfo', 'Awaiting medical report before proceeding.',
 '00000000-0000-0000-0000-000000000001', '2026-05-15');

INSERT INTO [Subjectivities] (SubjectivityID, SubmissionID, Description, DueDate, Status) VALUES
(NEWID(), 'A0000002-0002-0000-0000-000000000002',
 'Annual property inspection report within 30 days of inception.', '2026-07-31', 'Open'),
(NEWID(), 'A0000002-0002-0000-0000-000000000002',
 'Fire NOC certificate from local fire authority before policy activation.', '2026-06-15', 'Open');
GO

-- ============================================================
-- 6. POLICY BINDING, ISSUANCE & ENDORSEMENTS
-- ============================================================
USE [PolicyBindingIssuanceAndEndorsements];

DELETE FROM [Renewals];
DELETE FROM [Cancellations];
DELETE FROM [Endorsements];
DELETE FROM [Policies];

INSERT INTO [Policies] (PolicyID, SubmissionID, PolicyNumber, ProductLine, CoverageJSON, InceptionDate, ExpiryDate, Status) VALUES
('C0000001-0001-0000-0000-000000000001',
 'A0000001-0001-0000-0000-000000000001',
 'POL-2026-LIFE-00001',
 'Life',
 '{"sumInsured":2500000,"coverageType":"Term Life","policyTenure":20,"beneficiary":"Sunita Kumar","premiumAnnual":33925}',
 '2026-06-01', '2046-05-31', 'Active');
GO

-- ============================================================
-- VERIFY — Final row counts
-- ============================================================
SELECT 'Agent'             AS [Table], COUNT(*) AS [Rows] FROM [DistributionAndPartyManagement].dbo.[Agent]
UNION ALL SELECT 'CustomerParty',    COUNT(*) FROM [DistributionAndPartyManagement].dbo.[CustomerParty]
UNION ALL SELECT 'Submissions',      COUNT(*) FROM [SubmissionAndIntake].dbo.[Submissions]
UNION ALL SELECT 'Questionnaires',   COUNT(*) FROM [SubmissionAndIntake].dbo.[Questionnaires]
UNION ALL SELECT 'CompletenessChks', COUNT(*) FROM [SubmissionAndIntake].dbo.[CompletenessChecks]
UNION ALL SELECT 'RiskScores',       COUNT(*) FROM [RulesScoringAndReferralMatrix].dbo.[RiskScores]
UNION ALL SELECT 'Referrals',        COUNT(*) FROM [RulesScoringAndReferralMatrix].dbo.[Referrals]
UNION ALL SELECT 'Quotes',           COUNT(*) FROM [PricingQuotationAndTerms].dbo.[Quotes]
UNION ALL SELECT 'UWNotes',          COUNT(*) FROM [UnderwritingWorkflowAndDecisions].dbo.[UWNotes]
UNION ALL SELECT 'UWDecisions',      COUNT(*) FROM [UnderwritingWorkflowAndDecisions].dbo.[UWDecisions]
UNION ALL SELECT 'Subjectivities',   COUNT(*) FROM [UnderwritingWorkflowAndDecisions].dbo.[Subjectivities]
UNION ALL SELECT 'Policies',         COUNT(*) FROM [PolicyBindingIssuanceAndEndorsements].dbo.[Policies];
GO
