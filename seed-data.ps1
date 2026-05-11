# ================================================================
#  UnderwritePro — Indian Test Data Seeder
#  Seeds 10 records across all modules in correct order:
#  1. Agents (8082)  2. Submissions (8083)  3. Risk Scores (8085)
#  4. Pricing Params (8086)  5. Quotes (8086)
#
#  HOW TO RUN:
#    1. Start all services
#    2. Open PowerShell in UnderwritePro folder
#    3. .\seed-data.ps1
# ================================================================

$ErrorActionPreference = "Continue"

# ── Helpers ──────────────────────────────────────────────────────
function Post($url, $body) {
    try {
        $json = if ($body) { $body | ConvertTo-Json -Depth 5 } else { "{}" }
        return Invoke-RestMethod -Uri $url -Method POST -Body $json -ContentType "application/json"
    } catch {
        $msg = $_.Exception.Response | ForEach-Object {
            $reader = New-Object System.IO.StreamReader($_.GetResponseStream())
            $reader.ReadToEnd()
        }
        Write-Host "    ERROR calling $url : $msg" -ForegroundColor Red
        return $null
    }
}

function Show-Banner($text) {
    Write-Host ""
    Write-Host "─────────────────────────────────────────────" -ForegroundColor DarkCyan
    Write-Host "  $text" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────" -ForegroundColor DarkCyan
}

Write-Host ""
Write-Host "╔═════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   UnderwritePro — Indian Test Data Seeder  ║" -ForegroundColor Cyan
Write-Host "╚═════════════════════════════════════════════╝" -ForegroundColor Cyan

# ================================================================
# STEP 1 — AGENTS  (DistributionAndPartyManagement: 8082)
# ================================================================
Show-Banner "STEP 1: Creating 10 Agents (port 8082)"

$agentInputs = @(
    @{ Name="Rajesh Kumar";   ProducerCode="MUM-COM-001"; ContactInfo="rajesh.kumar@insureindia.com | +91-9876543210"; Region="Mumbai"    },
    @{ Name="Priya Sharma";   ProducerCode="DEL-HLT-001"; ContactInfo="priya.sharma@insureindia.com | +91-9876543211";  Region="Delhi"     },
    @{ Name="Suresh Patel";   ProducerCode="AHM-LIF-001"; ContactInfo="suresh.patel@insureindia.com | +91-9876543212";  Region="Ahmedabad" },
    @{ Name="Anita Desai";    ProducerCode="BLR-PNC-001"; ContactInfo="anita.desai@insureindia.com | +91-9876543213";   Region="Bangalore" },
    @{ Name="Vikram Singh";   ProducerCode="JAI-COM-002"; ContactInfo="vikram.singh@insureindia.com | +91-9876543214";  Region="Jaipur"    },
    @{ Name="Meera Nair";     ProducerCode="CHE-HLT-002"; ContactInfo="meera.nair@insureindia.com | +91-9876543215";    Region="Chennai"   },
    @{ Name="Amit Gupta";     ProducerCode="PUN-LIF-002"; ContactInfo="amit.gupta@insureindia.com | +91-9876543216";    Region="Pune"      },
    @{ Name="Kavya Reddy";    ProducerCode="HYD-PNC-002"; ContactInfo="kavya.reddy@insureindia.com | +91-9876543217";   Region="Hyderabad" },
    @{ Name="Rohit Malhotra"; ProducerCode="KOL-COM-003"; ContactInfo="rohit.malhotra@insureindia.com | +91-9876543218";Region="Kolkata"   },
    @{ Name="Sunita Verma";   ProducerCode="SUR-HLT-003"; ContactInfo="sunita.verma@insureindia.com | +91-9876543219";  Region="Surat"     }
)

$agentIds = @()
foreach ($a in $agentInputs) {
    $res = Post "http://localhost:8082/api/agents" $a
    if ($res -and $res.data -and $res.data.agentID) {
        $agentIds += $res.data.agentID
        Write-Host "  ✓ $($a.Name) ($($a.Region)) → $($res.data.agentID)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Failed: $($a.Name)" -ForegroundColor Red
        $agentIds += $null
    }
}

# ================================================================
# STEP 2 — SUBMISSIONS  (SubmissionAndIntake: 8083)
# ProductLine enum: 0=Life  1=Health  2=PnC  3=Commercial
# ================================================================
Show-Banner "STEP 2: Creating 10 Submissions (port 8083)"

$submissionInputs = @(
    @{ Party="PTY-MUM-0001"; Line=3; Sum=5000000;  Desc="Tata Steel Ltd - Factory Fire Insurance"         },
    @{ Party="PTY-DEL-0001"; Line=1; Sum=1000000;  Desc="Sharma Family - Group Health Coverage"           },
    @{ Party="PTY-AHM-0001"; Line=0; Sum=2500000;  Desc="Suresh Patel - 25 Yr Term Life Policy"          },
    @{ Party="PTY-BLR-0001"; Line=2; Sum=800000;   Desc="Anita Desai - Commercial Motor Fleet"            },
    @{ Party="PTY-JAI-0001"; Line=3; Sum=7500000;  Desc="Rajputana Textiles - Warehouse Property"         },
    @{ Party="PTY-CHE-0001"; Line=1; Sum=1500000;  Desc="Nair Multi-Specialty Hospital - Staff Health"    },
    @{ Party="PTY-PUN-0001"; Line=0; Sum=3000000;  Desc="Amit Gupta - Whole Life Endowment Plan"          },
    @{ Party="PTY-HYD-0001"; Line=2; Sum=600000;   Desc="Reddy Agro Farms - Crop & Livestock Cover"       },
    @{ Party="PTY-KOL-0001"; Line=3; Sum=10000000; Desc="Bengal Jute Co. - Marine Cargo Export"           },
    @{ Party="PTY-SUR-0001"; Line=1; Sum=1200000;  Desc="Verma Diamond Exports - Employee Health Plan"    }
)

$submissionIds = @()
for ($i = 0; $i -lt $submissionInputs.Count; $i++) {
    $s       = $submissionInputs[$i]
    $agentId = if ($i -lt $agentIds.Count -and $agentIds[$i]) { $agentIds[$i] } else { "AGT-FALLBACK-001" }

    $coverageJson = "{""sumInsured"": $($s.Sum), ""description"": ""$($s.Desc)""}"

    $body = @{
        PartyID       = $s.Party
        AgentID       = $agentId
        ProductLine   = $s.Line
        CoverageJSON  = $coverageJson
        InceptionDate = (Get-Date).AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ")
    }

    $res = Post "http://localhost:8083/api/submissions" $body
    if ($res -and $res.submissionID) {
        $submissionIds += $res.submissionID
        Write-Host "  ✓ $($s.Desc) → $($res.submissionID)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Failed: $($s.Desc)" -ForegroundColor Red
    }
}

# ================================================================
# STEP 3 — RISK SCORES  (RulesScoringAndReferralMatrix: 8085)
# POST /api/risk-scores/calculate/{submissionId}  — no body needed
# ================================================================
Show-Banner "STEP 3: Calculating Risk Scores (port 8085)"

$scoredIds = @()
foreach ($subId in $submissionIds) {
    $res = Post "http://localhost:8085/api/risk-scores/calculate/$subId" $null
    if ($res) {
        $band  = if ($res.band  -ne $null) { $res.band }  else { $res.Band }
        $score = if ($res.scoreValue -ne $null) { $res.scoreValue } else { $res.ScoreValue }
        Write-Host "  ✓ $subId → Band=$band  Score=$score" -ForegroundColor Green
        $scoredIds += $subId
    } else {
        Write-Host "  ✗ Failed risk score for: $subId" -ForegroundColor Red
    }
}

# ================================================================
# STEP 4 — PRICING PARAMS  (PricingQuotationAndTerms: 8086)
# ================================================================
Show-Banner "STEP 4: Seeding Pricing Parameters (port 8086)"

$from = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")

$params = @(
    # ── Base rates per product line (REQUIRED — missing = quote blocked) ──
    @{ ProductLine="Life";       ParamName="BaseRate";               Value=0.020; Description="Life insurance annual base rate 2%"              },
    @{ ProductLine="Health";     ParamName="BaseRate";               Value=0.035; Description="Health insurance annual base rate 3.5%"           },
    @{ ProductLine="PnC";        ParamName="BaseRate";               Value=0.015; Description="Property and Casualty base rate 1.5%"             },
    @{ ProductLine="Commercial"; ParamName="BaseRate";               Value=0.025; Description="Commercial insurance base rate 2.5%"              },

    # ── Risk loadings ──────────────────────────────────────────────────────
    @{ ProductLine="Global";     ParamName="RiskLoading_Medium";     Value=0.10;  Description="Medium risk band loading 10%"                     },
    @{ ProductLine="Global";     ParamName="RiskLoading_High";       Value=0.25;  Description="High risk band loading 25%"                       },

    # ── Occupation loadings ────────────────────────────────────────────────
    @{ ProductLine="Global";     ParamName="OccupationLoad_Mining";  Value=0.30;  Description="Mining sector occupation loading 30%"             },
    @{ ProductLine="Global";     ParamName="OccupationLoad_General"; Value=0.05;  Description="General sector occupation loading 5%"             },

    # ── Tenure discounts ───────────────────────────────────────────────────
    @{ ProductLine="Global";     ParamName="TenureDiscount_12m";     Value=0.03;  Description="1 year policy tenure discount 3%"                 },
    @{ ProductLine="Global";     ParamName="TenureDiscount_24m";     Value=0.05;  Description="2 year policy tenure discount 5%"                 },
    @{ ProductLine="Global";     ParamName="TenureDiscount_36m";     Value=0.07;  Description="3 year policy tenure discount 7%"                 },

    # ── Other discounts ────────────────────────────────────────────────────
    @{ ProductLine="Global";     ParamName="LoyaltyDiscount";        Value=0.05;  Description="Renewal loyalty discount 5%"                      },
    @{ ProductLine="Global";     ParamName="AgentDiscount_Preferred";Value=0.02;  Description="Preferred agent discount 2%"                      },

    # ── Tax (REQUIRED — missing = quote blocked) ───────────────────────────
    @{ ProductLine="Global";     ParamName="GstRate";                Value=0.18;  Description="GST rate on insurance premium 18%"                },

    # ── Minimum Premium per product line (REQUIRED — missing = quote blocked)
    @{ ProductLine="Life";       ParamName="MinimumPremium";         Value=1000;  Description="Life insurance minimum premium Rs. 1000"          },
    @{ ProductLine="Health";     ParamName="MinimumPremium";         Value=1500;  Description="Health insurance minimum premium Rs. 1500"         },
    @{ ProductLine="PnC";        ParamName="MinimumPremium";         Value=750;   Description="Property and Casualty minimum premium Rs. 750"     },
    @{ ProductLine="Commercial"; ParamName="MinimumPremium";         Value=5000;  Description="Commercial insurance minimum premium Rs. 5000"     },

    # ── Quote Validity Days per product line ───────────────────────────────
    @{ ProductLine="Life";       ParamName="QuoteValidityDays";      Value=90;    Description="Life quote valid for 90 days"                      },
    @{ ProductLine="Health";     ParamName="QuoteValidityDays";      Value=30;    Description="Health quote valid for 30 days"                    },
    @{ ProductLine="PnC";        ParamName="QuoteValidityDays";      Value=30;    Description="PnC quote valid for 30 days"                       },
    @{ ProductLine="Commercial"; ParamName="QuoteValidityDays";      Value=60;    Description="Commercial quote valid for 60 days"                }
)

foreach ($p in $params) {
    $body = @{
        ProductLine   = $p.ProductLine
        ParamName     = $p.ParamName
        Value         = $p.Value
        Description   = $p.Description
        EffectiveFrom = $from
    }
    $res = Post "http://localhost:8086/api/pricing-params" $body
    if ($res) {
        Write-Host "  ✓ $($p.ProductLine) / $($p.ParamName) = $($p.Value)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Failed (may already exist): $($p.ParamName)" -ForegroundColor Yellow
    }
}

# ================================================================
# STEP 5 — QUOTES  (PricingQuotationAndTerms: 8086)
# ================================================================
Show-Banner "STEP 5: Generating Quotes (port 8086)"

foreach ($subId in $scoredIds) {
    $body = @{
        SubmissionId = $subId
        RequestedBy  = "seeder-script"
    }
    $res = Post "http://localhost:8086/api/quotes" $body
    if ($res -and $res.totalPremium) {
        Write-Host "  ✓ Quote generated → Premium = Rs. $($res.totalPremium)  Status=$($res.status)" -ForegroundColor Green
    } elseif ($res) {
        Write-Host "  ✓ Quote generated for $subId" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Failed quote for: $subId" -ForegroundColor Red
    }
}

# ================================================================
# DONE
# ================================================================
Write-Host ""
Write-Host "╔═════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║           Seeding Complete!                 ║" -ForegroundColor Green
Write-Host "╚═════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Verify your data at:" -ForegroundColor White
Write-Host "  Agents:       http://localhost:8082/api/agents/search" -ForegroundColor Gray
Write-Host "  Submissions:  http://localhost:8083/api/submissions" -ForegroundColor Gray
Write-Host "  Risk Scores:  http://localhost:8085/api/risk-scores/band/0" -ForegroundColor Gray
Write-Host "  Params:       http://localhost:8086/api/pricing-params" -ForegroundColor Gray
Write-Host "  Quotes:       http://localhost:8086/api/pricing-params (check each submissionId)" -ForegroundColor Gray
Write-Host ""
