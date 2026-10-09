import { Injectable, computed, signal } from '@angular/core';
import {Chat, Message, User} from '../index';
import { toUtcMs } from '../utils/date';

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
      next.set(chatId, list.map(m => m.id === messageId ? { ...m, isDeleted: true, content: '', reactions: [] } : m));
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

  updateUserOnlineStatus(userId: string, isOnline: boolean, lastSeenAt?: string) {
    this._contacts.update(contacts =>
      contacts.map(u => (u.id === userId ? { ...u, isOnline, lastSeenAt: lastSeenAt ?? u.lastSeenAt } : u))
    );

    this._chats.update(chats =>
      chats.map(chat => ({
        ...chat,
        participants: chat.participants.map(p =>
          p.id === userId ? { ...p, isOnline, lastSeenAt: lastSeenAt ?? p.lastSeenAt } : p
        ),
      }))
    );
  }

  reset() {
    this._chats.set([]);
    this._messagesByChat.set(new Map());
    this._selectedChatId.set(null);
    this._contacts.set([]);
  }

  applyReceipt(chatId: string, userId: string, kind: 'delivered' | 'read', upTo: string) {
    const upToMs = toUtcMs(upTo);

    this._chats.update((list) =>
      list.map((c) => {
        if (c.id !== chatId) return c;

        const receipts = [...(c.receipts ?? [])];
        const i = receipts.findIndex((r) => r.userId === userId); // for this user in this chat (return -1 if not found)
        const next = { ...(i >= 0 ? receipts[i] : { userId, lastDeliveredAt: null, lastReadAt: null }) };

        if (kind === 'read' && upToMs > toUtcMs(next.lastReadAt)) next.lastReadAt = upTo;
        // for two cases
        if (upToMs > toUtcMs(next.lastDeliveredAt)) next.lastDeliveredAt = upTo;

        if (i >= 0) receipts[i] = next;
        else receipts.push(next);
        return { ...c, receipts };
      }),
    );
  }

    /**(idempotent)*/
  applyReaction(chatId: string, messageId: string, userId: string, emoji: string | null) {
    this._messagesByChat.update((map) => {
      const list = map.get(chatId);
      if (!list) return map;

      const next = new Map(map);
      next.set(
        chatId,
        list.map((m) => {
          if (m.id !== messageId) return m;
          const others = (m.reactions ?? []).filter((r) => r.userId !== userId);
          return { ...m, reactions: emoji ? [...others, { userId, emoji }] : others };
        }),
      );
      return next;
    });
  }
}