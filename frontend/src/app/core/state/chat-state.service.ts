import { Injectable, computed, signal } from '@angular/core';
import {Chat, Message, User} from '../index';


@Injectable({ 
  providedIn: 'root' 
})

export class ChatStateService {
  private readonly _chats = signal<Chat[]>([]);
  private readonly _messagesByChat = signal<Map<string, Message[]>>(new Map());
  private readonly _selectedChatId = signal<string | null>(null);
  private readonly _contacts = signal<User[]>([]);

  readonly chats = this._chats.asReadonly();
  readonly messagesByChat = this._messagesByChat.asReadonly();
  readonly selectedChatId = this._selectedChatId.asReadonly();
  readonly contacts = this._contacts.asReadonly();
  readonly selectedChat = computed(() => this._chats().find(c => c.id === this._selectedChatId()));
  readonly selectedMessages = computed(() => {
    const id = this._selectedChatId();
    return id ? this._messagesByChat().get(id) ?? [] : [];
  });

  // for setting state from outside (like from ChatFacade)
  setChats(chats: Chat[]) { this._chats.set(chats); }
  setContacts(users: User[]) { this._contacts.set(users); }
  selectChat(id: string | null) { this._selectedChatId.set(id); }

  addMessage(msg: Message) {
    this._messagesByChat.update(map => {
      const next = new Map(map);
      const list = next.get(msg.chatId) ?? [];
      if (list.some(m => m.id === msg.id)) return map; // avoid duplicate
      next.set(msg.chatId, [...list, msg]);
      return next;
    });
    this.updateLastMessage(msg);
  }

  updateMessage(msg: Message) {
    this._messagesByChat.update(map => {
      const next = new Map(map);
      const list = next.get(msg.chatId) ?? []; // for m the current chat
      next.set(msg.chatId, list.map(m => m.id === msg.id ? msg : m));
      return next;
    });
    this.updateLastMessage(msg);
  }

  softDeleteMessage(chatId: string, messageId: string){
    this._messagesByChat.update(map => {
      const next = new Map(map);
      const list = next.get(chatId) ?? [];
      next.set(chatId, list.map(m => m.id === messageId ? { ...m, isDeleted: true, content: '' } : m));
      return next;
    });
  }

  setMessages(chatId: string, messages: Message[]) {
    this._messagesByChat.update(map => new Map(map).set(chatId, messages));
  }

  incrementUnread(chatId: string) {
    this._chats.update(list => list.map(c => c.id === chatId ? 
      { ...c, unreadCount: c.unreadCount + 1 } : c));
  }

  clearUnread(chatId: string) {
    this._chats.update(list => list.map(c => 
      c.id === chatId ? { ...c, unreadCount: 0 } : c
    ));
  }

  private updateLastMessage(msg: Message) {
    this._chats.update(list => list.map(c => 
      c.id === msg.chatId ? { ...c, lastMessage: msg } : c
    ));
  }

  reset() {
    this._chats.set([]);
    this._messagesByChat.set(new Map());
    this._selectedChatId.set(null);
    this._contacts.set([]);
  }
}