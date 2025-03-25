import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap ,catchError, of, map} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {

  private notificationsUrl = 'https://localhost:7151/api/users/notifications';

  constructor(
    private http: HttpClient
  ) { }


  private getAuthHeaders(): HttpHeaders {
    const token = "";
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }


  getNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.notificationsUrl, { headers: this.getAuthHeaders() })
      .pipe(
        map((notifications) => notifications.map((n: any) => ({
          ...n,
          isUnRead: !n.isRead,
        }))),
        catchError(this.handleError<Notification[]>('getNotifications', []))
      );
  }
  
  

    private handleError<T>(operation = 'operation', result?: T) {
      return (error: any): Observable<T> => {
  
        console.error(error); 
  
        console.log(`${operation} failed: ${error.message}`);
  
        return of(result as T);
      };
    }

}