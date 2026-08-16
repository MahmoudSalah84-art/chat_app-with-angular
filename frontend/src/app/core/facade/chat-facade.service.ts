import { Injectable, inject, signal, computed } from '@angular/core';
import { ChatApiService ,ChatStateService, 
  SignalRService, AuthService, Chat, MessageType } from '../index';

@Injectable({ providedIn: 'root' })


export class ChatFacade {
  private readonly api = inject(ChatApiService);
  private readonly state = inject(ChatStateService);
  private readonly signalR = inject(SignalRService);
  private readonly auth = inject(AuthService);

  // ─── Expose State للـ Components ───
  readonly chats = this.state.chats;
  readonly selectedChat = this.state.selectedChat;
  readonly selectedMessages = this.state.selectedMessages;
  readonly contacts = this.state.contacts;
  readonly currentUser = this.auth.currentUser;

  constructor() {
    this.signalR.messageReceived$.subscribe(m => this.state.addMessage(m));
    this.signalR.messageEdited$.subscribe(m => this.state.updateMessage(m));
    this.signalR.messageDeleted$.subscribe(({ chatId, messageId }) => this.state.softDeleteMessage(chatId, messageId));
    this.signalR.chatCreated$.subscribe(() => this.loadChats());
    this.signalR.userTyping$.subscribe(({ chatId }) => this._typingChatId.set(chatId));
    this.signalR.userStoppedTyping$.subscribe(({ chatId }) => {
      if (this._typingChatId() === chatId) this._typingChatId.set(null)});
  }

  private readonly _searchQuery = signal('');
  private readonly _replyToMessageId = signal<string | null>(null);
  private readonly _editingMessageId = signal<string | null>(null);
  private readonly _typingChatId = signal<string | null>(null);

  readonly searchQuery = this._searchQuery.asReadonly();
  readonly replyToMessageId = this._replyToMessageId.asReadonly();
  readonly editingMessageId = this._editingMessageId.asReadonly();
  readonly typingChatId = this._typingChatId.asReadonly();
  readonly isOtherTyping = computed(() => this._typingChatId() === this.state.selectedChatId());

  readonly filteredChats = computed(() => {
    const q = this._searchQuery().trim().toLowerCase();
    if (!q) return this.state.chats();
    return this.state.chats().filter(c => 
      c.name.toLowerCase().includes(q) || 
      c.lastMessage?.content.toLowerCase().includes(q)
    );
  });

  readonly replyToMessage = computed(() => {
    const id = this._replyToMessageId();
    return id ? this.selectedMessages().find(m => m.id === id) : undefined;
  });

  readonly editingMessage = computed(() => {
    const id = this._editingMessageId();
    return id ? this.selectedMessages().find(m => m.id === id) : undefined;
  });

  // ─── Actions ───
  async loadChats() {
    const chats = await this.api.loadChats();
    this.state.setChats(chats);
  }

  async loadContacts() {
    const contacts = await this.api.loadContacts();
    this.state.setContacts(contacts);
  }

  async selectChat(chatId: string) {
    this.state.selectChat(chatId);
    this._replyToMessageId.set(null);
    this._editingMessageId.set(null);

    if (!this.state.messagesByChat().has(chatId)) {
      const messages = await this.api.loadMessages(chatId);
      this.state.setMessages(chatId, messages);
    }

    await this.markAsRead(chatId);
  }

  closeChat() { this.state.selectChat(null); }

  setSearchQuery(q: string) { this._searchQuery.set(q); }
  setReplyTo(id: string | null) { this._replyToMessageId.set(id); }
  cancelReply() { this._replyToMessageId.set(null); }
  startEdit(id: string) { this._editingMessageId.set(id); this._replyToMessageId.set(null); }
  cancelEdit() { this._editingMessageId.set(null); }

  async sendMessage(content: string) {
    const chatId = this.state.selectedChatId();
    if (!chatId) return;
    const replyTo = this._replyToMessageId();
    this._replyToMessageId.set(null);
    await this.signalR.sendMessage(chatId, MessageType.Text, content.trim(), replyTo);
  }

  async sendImage(dataUrl: string) {
    const chatId = this.state.selectedChatId();
    if (chatId) await this.signalR.sendMessage(chatId, MessageType.Image, dataUrl, null);
  }

  async saveEdit(newContent: string) {
    const chatId = this.state.selectedChatId();
    const msgId = this._editingMessageId();
    if (!chatId || !msgId) return;
    await this.signalR.editMessage(chatId, msgId, newContent);
    this._editingMessageId.set(null);
  }

  async deleteMessage(messageId: string) {
    const chatId = this.state.selectedChatId();
    if (chatId) await this.signalR.deleteMessage(chatId, messageId);
  }

  async startChatWithUser(userId: string) {
    const chat = await this.api.startDirectChat(userId);
    await this.afterChatCreated(chat);
  }

  async createGroup(name: string, avatarUrl: string, memberIds: string[]) {
    const chat = await this.api.createGroup(name, avatarUrl, memberIds);
    await this.afterChatCreated(chat);
  }

  async onTypingStart() {
    const id = this.state.selectedChatId();
    if (id) await this.signalR.startTyping(id);
  }

  async onTypingStop() {
    const id = this.state.selectedChatId();
    if (id) await this.signalR.stopTyping(id);
  }

  reset() {
    this.state.reset();
    this._searchQuery.set('');
    this._replyToMessageId.set(null);
    this._editingMessageId.set(null);
  }

  async sendImageMessage(dataUrl: string) {
    const chatId = this.state.selectedChatId();
    if (chatId) await this.signalR.sendMessage(chatId, MessageType.Image, dataUrl, null);
  }

  private async afterChatCreated(chat: Chat) {
    this.state.setChats([chat, ...this.state.chats().filter(c => c.id !== chat.id)]);
    await this.signalR.joinChatGroup(chat.id);
    await this.selectChat(chat.id);
  }

  private async markAsRead(chatId: string) {
    const messages = this.state.selectedMessages();
    const last = messages[messages.length - 1];
    if (!last) return;
    await this.signalR.markAsRead(chatId, last.id);
    this.state.clearUnread(chatId);
  }
}