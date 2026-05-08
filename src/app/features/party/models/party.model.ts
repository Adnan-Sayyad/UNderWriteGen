// Backend uses AgentID/PartyID (with capital D) → ASP.NET Core camelCase → agentID/partyID
export type AgentStatus = 'Active' | 'Inactive';
export type PartyType   = 'Individual' | 'Organization';
export type Segment     = 'Retail' | 'SME' | 'Corporate';
export type PartyStatus = 'Active' | 'Inactive';

export interface Agent {
  agentID: string;       // AGT-yyyyMMdd-XXXX
  name: string;
  producerCode: string;
  contactInfo: string | null;  // backend stores as plain string e.g. "Phone: +91..., Email: x@y"
  region: string | null;
  status: AgentStatus;
}

export interface CustomerParty {
  partyID: string;             // PTY-yyyyMMdd-XXXX
  partyType: PartyType;
  name: string;
  dOBIncorporation: string | null;  // backend: DOBIncorporation → camelCase: dOBIncorporation
  contactInfo: string | null;       // backend stores as plain string
  segment: Segment;
  status: PartyStatus;
}
