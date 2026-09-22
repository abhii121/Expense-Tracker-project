import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CsvImportResult, PagedResult, Transaction, TransactionQuery, TransactionRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly baseUrl = `${environment.apiUrl}/transactions`;

  constructor(private http: HttpClient) {}

  getAll(query: TransactionQuery): Observable<PagedResult<Transaction>> {
    let params = new HttpParams();
    if (query.from) params = params.set('from', query.from);
    if (query.to) params = params.set('to', query.to);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.type) params = params.set('type', query.type);
    params = params.set('page', query.page ?? 1);
    params = params.set('pageSize', query.pageSize ?? 20);

    return this.http.get<PagedResult<Transaction>>(this.baseUrl, { params });
  }

  create(request: TransactionRequest): Observable<Transaction> {
    return this.http.post<Transaction>(this.baseUrl, request);
  }

  update(id: number, request: TransactionRequest): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  importCsv(file: File): Observable<CsvImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<CsvImportResult>(`${this.baseUrl}/import`, formData);
  }
}
