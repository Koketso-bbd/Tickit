import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  private token: string | null = localStorage.getItem('jwt');
  private apiUrl: string = "https://localhost:7151/api/projects";
  
  constructor(private http: HttpClient) { }

  private setHeaders() {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.token}`,
      'Content-Type': 'application/json'
    });
  }

  getProjects(): Observable<any> {
    const headers = this.setHeaders();
    return this.http.get<any[]>(`${this.apiUrl}`, { headers });
  }

  getProjectById(id: number): Observable<any> {
    const headers = this.setHeaders();
    return this.http.get<any[]>(`${this.apiUrl}/${id}`, { headers });
  }

  createProject(projectData: { projectName: string, projectDescription: string }): Observable<any> {
    const headers = this.setHeaders();
    return this.http.post<any[]>(`${this.apiUrl}`, projectData , { headers });
  }

  deleteProject(id: number): Observable<any> {
    const headers = this.setHeaders();
    return this.http.delete<any[]>(`${this.apiUrl}/${id}`, {headers});
  }
}
