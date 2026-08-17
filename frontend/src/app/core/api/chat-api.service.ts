import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs'; 
import { environment } from '../../../environments/environment';
import { User, Message, Chat } from '../index';


@Injectable({ providedIn: 'root' }) //singleton service, available throughout the app

export class ChatApiService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  loadChats() {
    return firstValueFrom(this.http.get<Chat[]>(`${this.api}/chats`));
  }

  loadMessages(chatId: string) {
    return firstValueFrom(this.http.get<Message[]>(`${this.api}/chats/${chatId}/messages`));
  }

  loadContacts() {
    return firstValueFrom(this.http.get<User[]>(`${this.api}/users/contacts`));
  }

  startDirectChat(otherUserId: string) {
    return firstValueFrom(this.http.post<Chat>(`${this.api}/chats/direct`, { otherUserId }));
  }

 
  createGroup(name: string, avatarUrl: string, memberIds: string[]) {
    return firstValueFrom(this.http.post<Chat>(`${this.api}/chats/group`, { name, avatarUrl, memberIds }));
  }
  
  
}