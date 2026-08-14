import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Message } from '../models/message.model';
import { MessageType } from '../enums/message-type.enum';
import { AuthService } from './auth';

/**
 * كل تعامل مع مكتبة @microsoft/signalr محبوس هنا بس. باقي المشروع (زي
 * ChatService) بيشترك في Observables بسيطة (messageReceived$, userTyping$...)
 * من غير ما يعرف حاجة عن HubConnection أو الـ WebSocket نفسه.
 */
@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection: signalR.HubConnection | null = null;

  readonly messageReceived$ = new Subject<Message>();
  readonly messageEdited$ = new Subject<Message>();
  readonly messageDeleted$ = new Subject<{ chatId: string; messageId: string }>();
  readonly chatCreated$ = new Subject<string>();
  readonly userTyping$ = new Subject<{ chatId: string; userId: string }>();
  readonly userStoppedTyping$ = new Subject<{ chatId: string; userId: string }>();

  async connect(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        // بما إن المتصفح مش بيقدر يحط Authorization Header على اتصال
        // WebSocket، بنبعت التوكن كـ query string بدل كده - الـ Backend
        // مظبوط يقرأه من هناك تحديدًا (شوف Program.cs -> OnMessageReceived)
        accessTokenFactory: () => this.authService.getToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.registerEventHandlers();

    try {
      await this.hubConnection.start();
    } catch (error) {
      console.error('فشل الاتصال بـ SignalR:', error);
    }
  }

  async disconnect(): Promise<void> {
    await this.hubConnection?.stop();
    this.hubConnection = null;
  }

  private registerEventHandlers(): void {
    this.hubConnection?.on('MessageReceived', (message: Message) => this.messageReceived$.next(message));
    this.hubConnection?.on('MessageEdited', (message: Message) => this.messageEdited$.next(message));
    this.hubConnection?.on('MessageDeleted', (chatId: string, messageId: string) =>
      this.messageDeleted$.next({ chatId, messageId }),
    );
    this.hubConnection?.on('ChatCreated', (chatId: string) => this.chatCreated$.next(chatId));
    this.hubConnection?.on('UserTyping', (chatId: string, userId: string) =>
      this.userTyping$.next({ chatId, userId }),
    );
    this.hubConnection?.on('UserStoppedTyping', (chatId: string, userId: string) =>
      this.userStoppedTyping$.next({ chatId, userId }),
    );
  }

  sendMessage(chatId: string, type: MessageType, content: string, replyToMessageId: string | null): Promise<Message> {
    return this.invoke<Message>('SendMessage', chatId, type, content, replyToMessageId);
  }

  editMessage(chatId: string, messageId: string, newContent: string): Promise<Message> {
    return this.invoke<Message>('EditMessage', chatId, messageId, newContent);
  }

  deleteMessage(chatId: string, messageId: string): Promise<void> {
    return this.invoke<void>('DeleteMessage', chatId, messageId);
  }

  markAsRead(chatId: string, lastReadMessageId: string): Promise<void> {
    return this.invoke<void>('MarkAsRead', chatId, lastReadMessageId);
  }

  startTyping(chatId: string): Promise<void> {
    return this.invoke<void>('StartTyping', chatId);
  }

  stopTyping(chatId: string): Promise<void> {
    return this.invoke<void>('StopTyping', chatId);
  }

  joinChatGroup(chatId: string): Promise<void> {
    return this.invoke<void>('JoinChatGroup', chatId);
  }

  private invoke<T>(methodName: string, ...args: unknown[]): Promise<T> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      return Promise.reject('مفيش اتصال بالسيرفر دلوقتي');
    }
    return this.hubConnection.invoke<T>(methodName, ...args);
  }
}
