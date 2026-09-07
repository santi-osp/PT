import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { Center, CreateCustomerRequest, Customer, CustomerStatus, Neighborhood, ProblemDetails, UpdateCustomerRequest } from './models';

export class ApiError extends Error {
  constructor(public readonly status: number, public readonly problem: ProblemDetails) {
    super(problem.detail || problem.title || 'No fue posible completar la solicitud.');
  }
}

@Injectable({ providedIn: 'root' })
export class CustomerApi {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  search(search = '', status: CustomerStatus = 'all'): Observable<Customer[]> {
    const params = new HttpParams().set('search', search).set('status', status);
    return this.request(this.http.get<Customer[]>(`${this.base}/residential-customers`, { params }));
  }

  get(id: string): Observable<Customer> {
    return this.request(this.http.get<Customer>(`${this.base}/residential-customers/${id}`));
  }

  create(request: CreateCustomerRequest): Observable<Customer> {
    return this.request(this.http.post<Customer>(`${this.base}/residential-customers`, request));
  }

  update(id: string, request: UpdateCustomerRequest): Observable<Customer> {
    return this.request(this.http.put<Customer>(`${this.base}/residential-customers/${id}`, request));
  }

  retire(id: string): Observable<Customer> {
    return this.request(this.http.patch<Customer>(`${this.base}/residential-customers/${id}/retire`, {}));
  }

  centers(): Observable<Center[]> {
    return this.request(this.http.get<Center[]>(`${this.base}/catalogs/centers`));
  }

  neighborhoods(query = ''): Observable<Neighborhood[]> {
    return this.request(this.http.get<Neighborhood[]>(`${this.base}/catalogs/neighborhoods`, {
      params: new HttpParams().set('query', query)
    }));
  }

  private request<T>(source: Observable<T>): Observable<T> {
    return source.pipe(catchError((error: HttpErrorResponse) => {
      const problem = typeof error.error === 'object' && error.error ? error.error as ProblemDetails : {};
      return throwError(() => new ApiError(error.status, problem));
    }));
  }
}
