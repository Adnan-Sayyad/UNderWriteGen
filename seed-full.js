/**
 * UnderwritePro — Full Seed Script
 * ─────────────────────────────────────────────────────────────────────────────
 * HOW TO USE:
 *  1. DB wipe பண்ணிட்டு வா (all tables clear)
 *  2. Browser → localhost:4200 திற (login page)
 *  3. F12 → Console → 'allow pasting' type → Enter
 *  4. இந்த script paste → Enter
 *  5. ~2 minutes wait — done! ✅
 * ─────────────────────────────────────────────────────────────────────────────
 */

(async () => {
  const BASE = '/api';
  const PWD  = 'Test@1234';

  const log  = (msg) => console.log(msg);
  const err  = (msg) => console.error(msg);
  const wait = (ms)  => new Promise(r => setTimeout(r, ms));

  // ── Helpers ────────────────────────────────────────────────────────────────

  async function register(firstName, lastName, email, password = PWD) {
    const res  = await fetch(`${BASE}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ firstName, lastName, email, password, phoneNumber: '9876543210' }),
    });
    const data = await res.json();
    return data?.data ?? null;
  }

  async function login(email, password = PWD) {
    const res  = await fetch(`${BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    const data = await res.json();
    return data?.data ?? null;
  }

  async function assignRole(adminId, userId, role, token) {
    const res  = await fetch(`${BASE}/auth/assign-role`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify({ adminId, userId, roles: [role] }),
    });
    return await res.json();
  }

  async function createAgent(payload, token) {
    const res  = await fetch(`${BASE}/agents`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify(payload),
    });
    const data = await res.json();
    return data?.data ?? data;
  }

  async function createParty(payload, token) {
    const res  = await fetch(`${BASE}/parties`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify(payload),
    });
    const data = await res.json();
    return data?.data ?? data;
  }

  async function createSubmission(payload, token) {
    const res  = await fetch(`${BASE}/submissions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify(payload),
    });
    const data = await res.json();
    return data?.data ?? data;
  }

  // ── User definitions ───────────────────────────────────────────────────────

  const USERS = [
    { firstName: 'Admin',    lastName: 'One',     email: 'admin1@uwpro.com',   role: 'Admin'          },
    { firstName: 'Admin',    lastName: 'Two',     email: 'admin2@uwpro.com',   role: 'Admin'          },
    { firstName: 'Arjun',    lastName: 'Kumar',   email: 'agent1@uwpro.com',   role: 'Agent'          },
    { firstName: 'Priya',    lastName: 'Sharma',  email: 'agent2@uwpro.com',   role: 'Agent'          },
    { firstName: 'Rajan',    lastName: 'Menon',   email: 'uw1@uwpro.com',      role: 'Underwriter'    },
    { firstName: 'Kavitha',  lastName: 'Nair',    email: 'uw2@uwpro.com',      role: 'Underwriter'    },
    { firstName: 'Suresh',   lastName: 'Babu',    email: 'uwasst1@uwpro.com',  role: 'UWAssistant'    },
    { firstName: 'Meena',    lastName: 'Raj',     email: 'uwasst2@uwpro.com',  role: 'UWAssistant'    },
    { firstName: 'Vikram',   lastName: 'Das',     email: 'pricing1@uwpro.com', role: 'PricingAnalyst' },
    { firstName: 'Anitha',   lastName: 'Bose',    email: 'pricing2@uwpro.com', role: 'PricingAnalyst' },
    { firstName: 'Deepak',   lastName: 'Singh',   email: 'comp1@uwpro.com',    role: 'Compliance'     },
    { firstName: 'Lakshmi',  lastName: 'Iyer',    email: 'comp2@uwpro.com',    role: 'Compliance'     },
    { firstName: 'Karthik',  lastName: 'Pillai',  email: 'ops1@uwpro.com',     role: 'Operations'     },
    { firstName: 'Sowmya',   lastName: 'Reddy',   email: 'ops2@uwpro.com',     role: 'Operations'     },
  ];

  // ── Step 1: Register Admin first ───────────────────────────────────────────
  log('\n🚀 STEP 1 — Registering Admin...');
  const adminUser = await register('Admin', 'One', 'admin1@uwpro.com');
  if (!adminUser) { err('❌ Admin register failed! Stop.'); return; }
  log(`✅ Admin registered: ${adminUser.id}`);
  await wait(500);

  // ── Step 2: Login as Admin ─────────────────────────────────────────────────
  log('\n🔑 STEP 2 — Admin login...');
  const adminAuth = await login('admin1@uwpro.com');
  if (!adminAuth?.accessToken) { err('❌ Admin login failed! Stop.'); return; }
  const adminToken = adminAuth.accessToken;
  const adminId    = adminAuth.userId;
  log(`✅ Admin logged in: ${adminId}`);
  await wait(500);

  // ── Step 3: Assign Admin role to admin1 ────────────────────────────────────
  log('\n🎭 STEP 3 — Assigning Admin role...');
  await assignRole(adminId, adminUser.id, 'Admin', adminToken);
  log('✅ Admin role assigned');
  await wait(500);

  // ── Step 4: Register + assign roles for all other users ───────────────────
  log('\n👥 STEP 4 — Creating all users...');
  const userMap = {}; // email → { id, agentId }

  for (const u of USERS) {
    if (u.email === 'admin1@uwpro.com') {
      userMap[u.email] = { id: adminUser.id };
      continue;
    }
    try {
      const newUser = await register(u.firstName, u.lastName, u.email);
      if (!newUser) { err(`⚠️ Register failed: ${u.email}`); continue; }
      await assignRole(adminId, newUser.id, u.role, adminToken);
      userMap[u.email] = { id: newUser.id };
      log(`✅ ${u.firstName} ${u.lastName} (${u.role}) — ${u.email}`);
      await wait(300);
    } catch(e) {
      err(`❌ ${u.email}: ${e.message}`);
    }
  }

  // ── Step 5: Create Agents ──────────────────────────────────────────────────
  log('\n🏢 STEP 5 — Creating Agents...');
  const agentDefs = [
    { name: 'Arjun Kumar',  producerCode: 'PC-AGT-001', region: 'South', email: 'agent1@uwpro.com' },
    { name: 'Priya Sharma', producerCode: 'PC-AGT-002', region: 'North', email: 'agent2@uwpro.com' },
  ];

  const agentIdMap = {}; // email → agentID
  for (const a of agentDefs) {
    try {
      const agent = await createAgent({
        name:         a.name,
        producerCode: a.producerCode,
        region:       a.region,
        contactInfo:  JSON.stringify({ email: a.email, phone: '9876543210' }),
        status:       'Active',
      }, adminToken);
      const agentID = agent?.agentID ?? agent?.agentId ?? agent?.id ?? '';
      agentIdMap[a.email] = agentID;
      log(`✅ Agent: ${a.name} → ${agentID}`);
      await wait(300);
    } catch(e) {
      err(`❌ Agent ${a.name}: ${e.message}`);
    }
  }

  // ── Step 6: Create Customer Parties ───────────────────────────────────────
  log('\n👤 STEP 6 — Creating Customer Parties...');
  const partyDefs = [
    { name: 'Ramesh Iyer',     partyType: 'Individual', segment: 'Retail'       },
    { name: 'Sunita Mehra',    partyType: 'Individual', segment: 'Retail'       },
    { name: 'Tech Solutions',  partyType: 'Corporate',  segment: 'SME'          },
    { name: 'Global Finance',  partyType: 'Corporate',  segment: 'Corporate'    },
    { name: 'Arun Sharma',     partyType: 'Individual', segment: 'HighNetWorth' },
    { name: 'Sara Pillai',     partyType: 'Individual', segment: 'Retail'       },
    { name: 'BuildMax Corp',   partyType: 'Corporate',  segment: 'SME'          },
    { name: 'Preethi Nair',    partyType: 'Individual', segment: 'Retail'       },
    { name: 'Kiran Patel',     partyType: 'Individual', segment: 'HighNetWorth' },
    { name: 'Nexus Traders',   partyType: 'Corporate',  segment: 'Corporate'    },
  ];

  const partyIds = [];
  for (const p of partyDefs) {
    try {
      const party = await createParty({
        name:             p.name,
        partyType:        p.partyType,
        segment:          p.segment,
        status:           'Active',
        dOBIncorporation: '1990-01-01',
        contactInfo:      JSON.stringify({ email: `${p.name.toLowerCase().replace(/\s/g,'')}@mail.com`, phone: '9876543210' }),
      }, adminToken);
      const partyID = party?.partyID ?? party?.partyId ?? party?.id ?? '';
      partyIds.push(partyID);
      log(`✅ Party: ${p.name} → ${partyID}`);
      await wait(300);
    } catch(e) {
      err(`❌ Party ${p.name}: ${e.message}`);
    }
  }

  if (partyIds.length === 0) { err('❌ No parties created! Cannot create submissions.'); return; }

  // ── Step 7: Create 10 Submissions per Agent ────────────────────────────────
  log('\n📋 STEP 7 — Creating submissions (10 per agent)...');

  const PRODUCTS    = ['Life', 'Health', 'PnC', 'Commercial'];
  const agentEmails = ['agent1@uwpro.com', 'agent2@uwpro.com'];
  let totalSubs = 0;

  for (const agentEmail of agentEmails) {
    const agentID = agentIdMap[agentEmail];
    if (!agentID) { err(`⚠️ No agentID for ${agentEmail} — skipping`); continue; }

    // Login as agent to get their token
    const agentAuth = await login(agentEmail);
    if (!agentAuth?.accessToken) { err(`⚠️ Login failed for ${agentEmail}`); continue; }
    const agentToken = agentAuth.accessToken;

    log(`\n  📌 Creating 10 submissions for ${agentEmail} (${agentID})...`);

    for (let i = 0; i < 10; i++) {
      const partyID     = partyIds[i % partyIds.length];
      const productLine = PRODUCTS[i % PRODUCTS.length];
      const inceptionDate = new Date(Date.now() + (i + 1) * 30 * 24 * 60 * 60 * 1000)
                              .toISOString().split('T')[0]; // future dates

      try {
        const sub = await createSubmission({
          partyID,
          agentID,
          productLine,
          coverageJSON:  JSON.stringify({ sumInsured: (i + 1) * 100000 }),
          inceptionDate,
        }, agentToken);

        const subId = sub?.submissionId ?? sub?.submissionID ?? sub?.id ?? '?';
        log(`  ✅ Sub ${i+1}/10 — ${productLine} | Party: ${partyID?.slice(0,8)}… | ${subId?.slice(0,8)}…`);
        totalSubs++;
        await wait(200);
      } catch(e) {
        err(`  ❌ Sub ${i+1}: ${e.message}`);
      }
    }
  }

  // ── Done ───────────────────────────────────────────────────────────────────
  console.log('\n══════════════════════════════════════');
  console.log(`✅ Users created   : ${Object.keys(userMap).length}`);
  console.log(`✅ Agents created  : ${Object.keys(agentIdMap).length}`);
  console.log(`✅ Parties created : ${partyIds.length}`);
  console.log(`✅ Submissions     : ${totalSubs} (${totalSubs/2} per agent)`);
  console.log('══════════════════════════════════════');
  console.log('🎉 Seed complete! Login பண்ணி test பண்ணு!');
  console.log('\n📋 Login credentials:');
  console.log('   admin1@uwpro.com   → Admin');
  console.log('   agent1@uwpro.com   → Agent (10 submissions)');
  console.log('   agent2@uwpro.com   → Agent (10 submissions)');
  console.log('   uw1@uwpro.com      → Underwriter');
  console.log('   pricing1@uwpro.com → PricingAnalyst');
  console.log('   ops1@uwpro.com     → Operations');
  console.log('   Password: Test@1234');

})();
