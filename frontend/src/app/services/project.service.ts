import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  private token: string | null = localStorage.getItem('jwt');
  private apiUrl: string = "https://localhost:7151/api/projects";
  private apiUserProject: string = "https://localhost:7151/api/userprojects"
  
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

  updateProject(id: number, projectData: { projectName: string, projectDescription: string }): Observable<any> {
    const headers = this.setHeaders();
    return this.http.patch<any>(`${this.apiUrl}/${id}`, projectData, { headers });
  }

  addUserToProject(userId: number, projectId: number, roleId: number): Observable<any> {
    const headers = this.setHeaders();
    const params = new HttpParams()
    .set('userId', userId)
    .set('projectId', projectId)
    .set('roleId', roleId);
    
    return this.http.post(`${this.apiUserProject}`, null, { headers, params });
  }

  removeUserFromProject(userId: number, projectId: number) {
    const headers = this.setHeaders();
    const params = new HttpParams()
    .set('userId', userId)
    .set('projectId', projectId);

    return this.http.delete(`${this.apiUserProject}`, { headers, params});
  }

  updateUserRole(userId: number, projectId: number, newRoleId: number) {
    const headers = this.setHeaders();
    const params = new HttpParams()
    .set('userId', userId)
    .set('projectId', projectId)
    .set('newRoleId', newRoleId);

    return this.http.put(`${this.apiUserProject}`, null, {headers, params})
  }
}
