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

  uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return firstValueFrom(this.http.post<{
      url: string;
      fileName: string;
      fileSize: number;
      contentType: string;
      messageType: number;
    }>(`${this.api}/upload`, formData));
  }
  
  uploadAttachment(chatId: string, file: File, replyToMessageId?: string | null, caption?: string) {
  const form = new FormData();
  form.append('chatId', chatId);
  form.append('file', file, file.name);
  if (caption) form.append('caption', caption);
  if (replyToMessageId) form.append('replyToMessageId', replyToMessageId);

  return this.http.post<Message>(`${this.api}/messages/attachments`, form, {
    reportProgress: true, // to get progress events (0-100%)
    observe: 'events', 
  });
  }
}