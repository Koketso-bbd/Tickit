import { Injectable } from '@angular/core';
import { User } from './user.interface';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private user!: User;

  private userUrl = 'https://localhost:7151/api/users';

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

  getUsertById(userId: number): Observable<User> {
    return this.http.get<User>(`${this.userUrl}/${userId}`, { headers: this.getAuthHeaders() });
  }
}
