/**
 * UnderwritePro — Seed Test Users
 * ─────────────────────────────────────────────────────────────────────────────
 * HOW TO USE:
 *  1. Open browser → Login as Admin
 *  2. Open DevTools → Console (F12)
 *  3. Paste this entire script → Press Enter
 *  4. Wait — all 14 users will be created automatically ✅
 * ─────────────────────────────────────────────────────────────────────────────
 */

(async () => {
  const BASE     = '/api';
  const TOKEN    = localStorage.getItem('uwpro_token');
  const USER_RAW = localStorage.getItem('uwpro_user');

  if (!TOKEN || !USER_RAW) {
    console.error('❌ Not logged in! Please login as Admin first.');
    return;
  }

  const adminId = JSON.parse(USER_RAW).userId;
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${TOKEN}`,
  };

  const USERS = [
    // Agent
    { firstName: 'Arjun',   lastName: 'Kumar',  email: 'agent1@uwpro.com',   password: 'Test@1234', role: 'Agent' },
    { firstName: 'Priya',   lastName: 'Sharma', email: 'agent2@uwpro.com',   password: 'Test@1234', role: 'Agent' },
    // Underwriter
    { firstName: 'Rajan',   lastName: 'Menon',  email: 'uw1@uwpro.com',      password: 'Test@1234', role: 'Underwriter' },
    { firstName: 'Kavitha', lastName: 'Nair',   email: 'uw2@uwpro.com',      password: 'Test@1234', role: 'Underwriter' },
    // UWAssistant
    { firstName: 'Suresh',  lastName: 'Babu',   email: 'uwasst1@uwpro.com',  password: 'Test@1234', role: 'UWAssistant' },
    { firstName: 'Meena',   lastName: 'Raj',    email: 'uwasst2@uwpro.com',  password: 'Test@1234', role: 'UWAssistant' },
    // PricingAnalyst
    { firstName: 'Vikram',  lastName: 'Das',    email: 'pricing1@uwpro.com', password: 'Test@1234', role: 'PricingAnalyst' },
    { firstName: 'Anitha',  lastName: 'Bose',   email: 'pricing2@uwpro.com', password: 'Test@1234', role: 'PricingAnalyst' },
    // Compliance
    { firstName: 'Deepak',  lastName: 'Singh',  email: 'comp1@uwpro.com',    password: 'Test@1234', role: 'Compliance' },
    { firstName: 'Lakshmi', lastName: 'Iyer',   email: 'comp2@uwpro.com',    password: 'Test@1234', role: 'Compliance' },
    // Operations
    { firstName: 'Karthik', lastName: 'Pillai', email: 'ops1@uwpro.com',     password: 'Test@1234', role: 'Operations' },
    { firstName: 'Sowmya',  lastName: 'Reddy',  email: 'ops2@uwpro.com',     password: 'Test@1234', role: 'Operations' },
    // Admin
    { firstName: 'Admin',   lastName: 'One',    email: 'admin1@uwpro.com',   password: 'Test@1234', role: 'Admin' },
    { firstName: 'Admin',   lastName: 'Two',    email: 'admin2@uwpro.com',   password: 'Test@1234', role: 'Admin' },
  ];

  let success = 0;
  let failed  = 0;

  console.log('🚀 Starting seed — 14 users...\n');

  for (const u of USERS) {
    try {
      // Step 1: Register
      const regRes = await fetch(`${BASE}/auth/register`, {
        method: 'POST',
        headers,
        body: JSON.stringify({
          firstName:   u.firstName,
          lastName:    u.lastName,
          email:       u.email,
          password:    u.password,
          phoneNumber: '9876543210',
        }),
      });
      const regData = await regRes.json();

      if (!regData.success || !regData.data) {
        console.warn(`⚠️  ${u.email} — Register failed: ${regData.message}`);
        failed++;
        continue;
      }

      const userId = regData.data.id ?? regData.data.userId;

      // Step 2: Assign Role
      const roleRes = await fetch(`${BASE}/auth/assign-role`, {
        method: 'POST',
        headers,
        body: JSON.stringify({ adminId, userId, roles: [u.role] }),
      });
      const roleData = await roleRes.json();

      if (!roleData.success) {
        console.warn(`⚠️  ${u.email} — Role assign failed: ${roleData.message}`);
        failed++;
        continue;
      }

      console.log(`✅ ${u.firstName} ${u.lastName} (${u.role}) — ${u.email}`);
      success++;

    } catch (err) {
      console.error(`❌ ${u.email} — Error:`, err);
      failed++;
    }
  }

  console.log(`\n─────────────────────────────────`);
  console.log(`✅ Created : ${success}`);
  console.log(`❌ Failed  : ${failed}`);
  console.log(`─────────────────────────────────`);
  console.log('Done! Refresh Admin → User Management to see all users.');
})();
