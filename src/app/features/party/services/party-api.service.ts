import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Agent, CustomerParty, ContactInfo, PartyType, Segment, PartyStatus, AgentStatus } from '../models/party.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';

function parseContactInfo(raw: any): ContactInfo {
  if (!raw) return { email: '', phone: '' };
  if (typeof raw === 'string') {
    try { return JSON.parse(raw); } catch { return { email: '', phone: raw }; }
  }
  return raw;
}

function normalizeAgent(r: any): Agent {
  return {
    agentId:      r.agentId      ?? r.agentID      ?? r.AgentID      ?? '',
    name:         r.name         ?? r.Name         ?? '',
    producerCode: r.producerCode ?? r.ProducerCode ?? '',
    contactInfo:  parseContactInfo(r.contactInfo   ?? r.ContactInfo),
    region:       r.region       ?? r.Region       ?? '',
    status:       (r.status      ?? r.Status       ?? 'Active') as AgentStatus,
  };
}

function normalizeParty(r: any): CustomerParty {
  return {
    partyId:          r.partyId          ?? r.partyID          ?? r.PartyID          ?? '',
    partyType:        (r.partyType        ?? r.PartyType        ?? 'Individual')       as PartyType,
    name:             r.name             ?? r.Name             ?? '',
    dobIncorporation: r.dobIncorporation ?? r.DOBIncorporation ?? r.dOBIncorporation  ?? '',
    contactInfo:      parseContactInfo(r.contactInfo ?? r.ContactInfo),
    segment:          (r.segment          ?? r.Segment          ?? 'Retail')           as Segment,
    status:           (r.status           ?? r.Status           ?? 'Active')           as PartyStatus,
  };
}

function toPagedAgents(res: any): PagedResponse<Agent> {
  const raw: any[] = Array.isArray(res) ? res : (res?.data ?? res?.content ?? []);
  const items = raw.map(normalizeAgent);
  return {
    content: items, data: items,
    totalPages: 1, totalElements: items.length, size: items.length, number: 0,
  } as unknown as PagedResponse<Agent>;
}

function toPagedParties(res: any): PagedResponse<CustomerParty> {
  const raw: any[] = Array.isArray(res) ? res : (res?.data ?? res?.content ?? []);
  const items = raw.map(normalizeParty);
  return {
    content: items, data: items,
    totalPages: 1, totalElements: items.length, size: items.length, number: 0,
  } as unknown as PagedResponse<CustomerParty>;
}

@Injectable({ providedIn: 'root' })
export class PartyApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  // ── Agents (GET /api/agents/search) ───────────────────────────────────────

  getAgents(req?: any, filters?: Record<string, string>) {
    const params: any = {};
    if (filters?.['name'])   params['name']   = filters['name'];
    if (filters?.['region']) params['region'] = filters['region'];
    if (filters?.['status']) params['status'] = filters['status'];
    return this.http.get<any>(`${this.base}/agents/search`, { params }).pipe(
      map(res => toPagedAgents(res))
    );
  }

  getAgent(id: string) {
    return this.http.get<any>(`${this.base}/agents/${id}`).pipe(
      map(res => ({ data: normalizeAgent(res?.data ?? res) } as ApiResponse<Agent>))
    );
  }

  createAgent(payload: Partial<Agent>) {
    return this.http.post<ApiResponse<Agent>>(`${this.base}/agents`, {
      name:         payload.name,
      producerCode: payload.producerCode,
      contactInfo:  payload.contactInfo ? JSON.stringify(payload.contactInfo) : null,
      region:       payload.region,
    });
  }

  updateAgent(id: string, payload: Partial<Agent>) {
    return this.http.put<ApiResponse<Agent>>(`${this.base}/agents/${id}`, {
      name:        payload.name,
      contactInfo: payload.contactInfo ? JSON.stringify(payload.contactInfo) : null,
      region:      payload.region,
    });
  }

  // ── Customer Parties (GET /api/customerparties/search) ────────────────────

  getParties(req?: any, filters?: Record<string, string>) {
    const params: any = {};
    if (filters?.['name'])      params['name']      = filters['name'];
    if (filters?.['partyType']) params['partyType'] = filters['partyType'];
    if (filters?.['segment'])   params['segment']   = filters['segment'];
    if (filters?.['status'])    params['status']    = filters['status'];
    return this.http.get<any>(`${this.base}/customerparties/search`, { params }).pipe(
      map(res => toPagedParties(res))
    );
  }

  getParty(id: string) {
    return this.http.get<any>(`${this.base}/customerparties/${id}`).pipe(
      map(res => ({ data: normalizeParty(res?.data ?? res) } as ApiResponse<CustomerParty>))
    );
  }

  createParty(payload: Partial<CustomerParty>) {
    return this.http.post<ApiResponse<CustomerParty>>(`${this.base}/customerparties`, {
      partyType:        payload.partyType,
      name:             payload.name,
      dobIncorporation: payload.dobIncorporation || null,
      contactInfo:      payload.contactInfo ? JSON.stringify(payload.contactInfo) : null,
      segment:          payload.segment,
    });
  }

  updateParty(id: string, payload: Partial<CustomerParty>) {
    return this.http.put<ApiResponse<CustomerParty>>(`${this.base}/customerparties/${id}`, {
      name:             payload.name,
      dobIncorporation: payload.dobIncorporation || null,
      contactInfo:      payload.contactInfo ? JSON.stringify(payload.contactInfo) : null,
      segment:          payload.segment,
    });
  }
}
