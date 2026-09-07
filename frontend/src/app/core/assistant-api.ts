import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiError } from './customer-api';
import { AssistantMessageRequest, AssistantMessageResponse, ProblemDetails } from './models';

@Injectable({ providedIn: 'root' })
export class AssistantApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/assistant`;

  message(request: AssistantMessageRequest): Observable<AssistantMessageResponse> {
    return this.request(this.http.post<AssistantMessageResponse>(`${this.base}/message`, request));
  }

  confirm(token: string): Observable<AssistantMessageResponse> {
    return this.request(this.http.post<AssistantMessageResponse>(`${this.base}/confirm`, { token }));
  }

  private request<T>(source: Observable<T>): Observable<T> {
    return source.pipe(catchError((error: HttpErrorResponse) => {
      const problem = typeof error.error === 'object' && error.error ? error.error as ProblemDetails : {};
      return throwError(() => new ApiError(error.status, problem));
    }));
  }
}
