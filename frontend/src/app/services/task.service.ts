import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs'

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private apiUrl = 'https://localhost:7151/api';

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = '';
    return new HttpHeaders({
      'content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    });
  }

  fetchData(endpoint: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${endpoint}`, { headers: this.getHeaders() });
  }

  postData(endpoint: string, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${endpoint}`, data, { headers: this.getHeaders() });
  }
}
