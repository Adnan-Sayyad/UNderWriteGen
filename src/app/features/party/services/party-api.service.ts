import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Agent, CustomerParty } from '../models/party.model';
import { ApiResponse } from '../../../shared/models/api-response.model';
 
@Injectable({ providedIn: 'root' })
export class PartyApiService {
  private readonly base = `${environment.apiBaseUrl}`;
 
  constructor(private http: HttpClient) {}
 
  // Agents — backend: GET /api/agents/search?name=&region=&status=
  getAgents(name = '', region = '', status = '') {
    let params = new HttpParams();
    if (name)   params = params.set('name', name);
    if (region) params = params.set('region', region);
    if (status) params = params.set('status', status);
    return this.http.get<ApiResponse<Agent[]>>(`${this.base}/agents/search`, { params });
  }
 
  getAgent(id: string) {
    return this.http.get<ApiResponse<Agent>>(`${this.base}/agents/${id}`);
  }
 
  createAgent(payload: Partial<Agent>) {
    return this.http.post<ApiResponse<Agent>>(`${this.base}/agents`, payload);
  }
 
  updateAgent(id: string, payload: Partial<Agent>) {
    return this.http.put<ApiResponse<Agent>>(`${this.base}/agents/${id}`, payload);
  }
 
  activateAgent(id: string) {
    return this.http.patch<ApiResponse<Agent>>(`${this.base}/agents/${id}/activate`, {});
  }
 
  deactivateAgent(id: string) {
    return this.http.patch<ApiResponse<Agent>>(`${this.base}/agents/${id}/deactivate`, {});
  }
 
  // Customer Parties — backend: GET /api/customerparties/search (proxied via /parties)
  getParties(name = '', partyType = '', segment = '', status = '') {
    let params = new HttpParams();
    if (name)      params = params.set('name', name);
    if (partyType) params = params.set('partyType', partyType);
    if (segment)   params = params.set('segment', segment);
    if (status)    params = params.set('status', status);
    return this.http.get<ApiResponse<CustomerParty[]>>(`${this.base}/parties/search`, { params });
  }
 
  getParty(id: string) {
    return this.http.get<ApiResponse<CustomerParty>>(`${this.base}/parties/${id}`);
  }
 
  createParty(payload: Partial<CustomerParty>) {
    return this.http.post<ApiResponse<CustomerParty>>(`${this.base}/parties`, payload);
  }
 
  updateParty(id: string, payload: Partial<CustomerParty>) {
    return this.http.put<ApiResponse<CustomerParty>>(`${this.base}/parties/${id}`, payload);
  }
 
  activateParty(id: string) {
    return this.http.patch<ApiResponse<CustomerParty>>(`${this.base}/parties/${id}/activate`, {});
  }
 
  deactivateParty(id: string) {
    return this.http.patch<ApiResponse<CustomerParty>>(`${this.base}/parties/${id}/deactivate`, {});
  }
}