import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PricingParam, Quote } from '../models/pricing.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class PricingApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  getParams(productLine?: string) {
    const params = productLine ? new HttpParams().set('productLine', productLine) : undefined;
    return this.http.get<ApiResponse<PricingParam[]>>(`${this.base}/pricing-params`, { params });
  }
  saveParam(payload: Partial<PricingParam>) { return this.http.post<ApiResponse<PricingParam>>(`${this.base}/pricing-params`, payload); }
  updateParam(id: string, payload: Partial<PricingParam>) { return this.http.put<ApiResponse<PricingParam>>(`${this.base}/pricing-params/${id}`, payload); }

  getQuotes(req: PageRequest, filters?: Record<string, string>) {
    return this.http.get<PagedResponse<Quote>>(`${this.base}/quotes`, { params: new HttpParams({ fromObject: { ...req, ...filters } }) });
  }
  getQuote(id: string) { return this.http.get<ApiResponse<Quote>>(`${this.base}/quotes/${id}`); }
  createQuote(payload: Partial<Quote>) { return this.http.post<ApiResponse<Quote>>(`${this.base}/quotes`, payload); }
  updateQuote(id: string, payload: Partial<Quote>) { return this.http.put<ApiResponse<Quote>>(`${this.base}/quotes/${id}`, payload); }
  acceptQuote(id: string) { return this.http.post<ApiResponse<Quote>>(`${this.base}/quotes/${id}/accept`, {}); }
}
