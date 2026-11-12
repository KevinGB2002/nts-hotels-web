import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/api/users`;

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.url);
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.url}/${id}`);
  }

  createUser(user: Partial<User>): Observable<User> {
    return this.http.post<User>(this.url, user);
  }

  updateUser(id: string, user: Partial<User>): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, user);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
