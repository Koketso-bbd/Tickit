import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Task } from '../interfaces/task.interface';

@Injectable({
  providedIn: 'root'
})
export class TaskViewService {
  private task!: Task;

  private taskUrl = 'https://localhost:7151/api/tasks';

  constructor(
    private http: HttpClient
  ) { }


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

  getTaskById(taskId: number): Observable<Task> { 
    return this.http.get<Task>(`${this.taskUrl}/${ taskId }`, { headers: this.getAuthHeaders() });
  }
}
