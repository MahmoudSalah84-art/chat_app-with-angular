import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User} from '../index';


@Injectable({ providedIn: 'root' })
export class UserApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getProfile(): Promise<User> {
    return firstValueFrom(this.http.get<User>(`${this.apiUrl}/users/me`));
  }

  updateProfile(payload: { name?: string; about?: string; avatarUrl?: string }): Promise<User> {
    return firstValueFrom(this.http.put<User>(`${this.apiUrl}/users/me`, payload));
  }

  searchUsers(query: string): Promise<User[]> {
    return firstValueFrom(
      this.http.get<User[]>(`${this.apiUrl}/users/search`, { params: { q: query } }),
    );
  }
}