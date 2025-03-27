import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Task } from '../interfaces/task.interface';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class TaskFilterService {

  private taskUrl = 'https://localhost:7151/api/tasks'; // Use http if https is not set up

  constructor(private http: HttpClient) { }

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
    return this.http.get<Task[]>(this.taskUrl, { headers: this.getAuthHeaders() }).pipe(
      tap((tasks) => console.log('Fetched tasks:', tasks)) 
    );
  }
}
