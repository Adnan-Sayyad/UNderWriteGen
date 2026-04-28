import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Agent, CustomerParty } from '../models/party.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class PartyApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  // Agents
  getAgents(req: PageRequest) {
    const params = new HttpParams({ fromObject: { ...req } });
    return this.http.get<PagedResponse<Agent>>(`${this.base}/agents`, { params });
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

  // Customer Parties
  getParties(req: PageRequest) {
    const params = new HttpParams({ fromObject: { ...req } });
    return this.http.get<PagedResponse<CustomerParty>>(`${this.base}/parties`, { params });
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
}
