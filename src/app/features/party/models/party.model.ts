// Backend uses AgentID/PartyID (with capital D) → ASP.NET Core camelCase → agentID/partyID
export type AgentStatus = 'Active' | 'Inactive';
export type PartyType   = 'Individual' | 'Corporation';   // backend: 'Corporation' (not 'Organization')
export type Segment     = 'Retail' | 'SME' | 'Corporate';
export type PartyStatus = 'Active' | 'Inactive';

export interface Agent {
  agentID: string;       // AGT-yyyyMMdd-XXXX
  name: string;
  producerCode: string;
  contactInfo: string | null;  // phone (10 digits, no leading 0) OR email
  region: string | null;
  status: AgentStatus;
}

export interface CustomerParty {
  partyID: string;             // PTY-yyyyMMdd-XXXX
  partyType: PartyType;        // 'Individual' | 'Corporation'
  name: string;
  dOBIncorporation: string | null;  // backend: DOBIncorporation → camelCase: dOBIncorporation
  contactInfo: string | null;       // phone (10 digits, no leading 0) OR email
  segment: Segment;
  status: PartyStatus;
}
