import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import {AuthService, Message, MessageType} from '../index';

@Injectable({
  providedIn: 'root',
})

export class SignalRService {
  private readonly authService = inject(AuthService);
  
  private hubConnection: signalR.HubConnection | null = null;
  private connectingPromise: Promise<void> | null = null;

  readonly messageReceived$ = new Subject<Message>();
  readonly messageEdited$ = new Subject<Message>();
  readonly messageDeleted$ = new Subject<{ chatId: string; messageId: string }>();
  readonly chatCreated$ = new Subject<string>();
  readonly userTyping$ = new Subject<{ chatId: string; userId: string }>();
  readonly userStoppedTyping$ = new Subject<{ chatId: string; userId: string }>();

  async connect(): Promise<void> {
    // Already connected
    if (
      this.hubConnection?.state ===
      signalR.HubConnectionState.Connected
    ) {
      return;
    }

    // Already connecting → wait for the same connection
    if (this.connectingPromise) {
      return this.connectingPromise;
    }

    // Create connection only once
    if (!this.hubConnection) {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(environment.hubUrl, {
          accessTokenFactory: () =>
            this.authService.getToken() ?? '',
        })
        .withAutomaticReconnect()
        .build();

      this.registerEventHandlers();

      this.hubConnection.onreconnecting(error => {
        console.log('SignalR reconnecting...', error);
      });

      this.hubConnection.onreconnected(connectionId => {
        console.log('SignalR reconnected:', connectionId);
      });

      this.hubConnection.onclose(error => {
        console.log('SignalR closed:', error);
      }
    ); 
  }

  this.connectingPromise = this.hubConnection.start();

  try {
    await this.connectingPromise;
    console.log('SignalR connected');
  } catch (error) {
    this.hubConnection = null;
    console.error('SignalR connection failed:', error);
    throw error;
  } finally {
    this.connectingPromise = null;
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
      this.messageDeleted$.next( { chatId, messageId}),
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

  private async invoke<T>( methodName: string, ...args: unknown[]): Promise<T> {
  await this.connect();
  return this.hubConnection!.invoke<T>(  methodName, ...args );
  }
}