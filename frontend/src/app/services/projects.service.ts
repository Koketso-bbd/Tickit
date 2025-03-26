import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Project } from '../interfaces/project.interface';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {
  private project!: Project;

  private projectUrl = 'https://localhost:7151/api/projects';

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

  getProjectById(projectId: number): Observable<Project> {
    return this.http.get<Project>(`${this.projectUrl}/${projectId}`, { headers: this.getAuthHeaders() });
  }
}
