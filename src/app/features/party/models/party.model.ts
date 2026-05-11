export type AgentStatus = 'Active' | 'Inactive';
export type PartyType   = 'Individual' | 'Corporate' | 'SME' | 'Corporation';
export type Segment     = 'Retail' | 'SME' | 'Corporate' | 'HighNetWorth';
export type PartyStatus = 'Active' | 'Inactive';

export interface ContactInfo {
  email: string;
  phone: string;
}

export interface Agent {
  agentID:      string;
  name:         string;
  producerCode: string;
  contactInfo:  ContactInfo;
  region:       string;
  status:       AgentStatus;
}

export interface CustomerParty {
  partyID:          string;
  partyType:        PartyType;
  name:             string;
  dOBIncorporation: string | null;
  contactInfo:      ContactInfo;
  segment:          Segment;
  status:           PartyStatus;
}
