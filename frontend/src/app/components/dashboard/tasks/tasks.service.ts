import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Task } from '../../../interfaces/task.interface';

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  private apiUrl = 'https://localhost:7151/api/tasks'; 

  constructor(private http: HttpClient) {}

  
  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }

  getUserTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(this.apiUrl,{ headers: this.getAuthHeaders()});
  }
  catch(error: any): Observable<Task[]> {
    console.error(error);
    return of([]);
  }
}
