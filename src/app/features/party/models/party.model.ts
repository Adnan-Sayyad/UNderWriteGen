export type AgentStatus = 'Active' | 'Inactive' | 'Suspended';
export type PartyType = 'Individual' | 'Organization';
export type Segment = 'Retail' | 'SME' | 'Corporate';
export type PartyStatus = 'Active' | 'Inactive';

export interface Agent {
  agentId: string;
  name: string;
  producerCode: string;
  contactInfo: ContactInfo;
  region: string;
  status: AgentStatus;
}

export interface CustomerParty {
  partyId: string;
  partyType: PartyType;
  name: string;
  dobIncorporation: string;
  contactInfo: ContactInfo;
  segment: Segment;
  status: PartyStatus;
}

export interface ContactInfo {
  email: string;
  phone: string;
  address?: string;
  city?: string;
  state?: string;
  pinCode?: string;
}
