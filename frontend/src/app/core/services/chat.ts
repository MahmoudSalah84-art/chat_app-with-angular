import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Chat } from '../models/chat.model';
import { Message } from '../models/message.model';
import { User } from '../models/user.model';
import { MessageType } from '../enums/message-type.enum';
import { AuthService } from './auth';
import { SignalRService } from './signalr';

/**
 * ChatService دلوقتي بيقسم شغله بوضوح:
 * - القراءة الأولية (لما تفتح شات لأول مرة) → HTTP عبر HttpClient
 * - أي تحديث لحظي بعد كده (رسالة جديدة، تعديل، حذف، كتابة) → SignalR
 *
 * لاحظ إننا "منضيفش" الرسالة محليًا فورًا لما نبعتها (Optimistic Update) -
 * بدل كده بنستنى event "MessageReceived" من السيرفر (اللي بيوصلنا إحنا
 * كمان لأننا عضو في نفس الـ Group). ده بيمنع مشكلة شائعة: ظهور الرسالة
 * مرتين (مرة محلي، ومرة من السيرفر).
 */
@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly signalR = inject(SignalRService);

  /** بنعرضه بنفس الاسم القديم عشان باقي الـ Components متتغيرش */
  readonly currentUser = this.authService.currentUser;

  private readonly _chats = signal<Chat[]>([]);
  readonly chats = this._chats.asReadonly();

  /** كل الرسائل مجمعة حسب chatId - بنحمّلها أول ما تفتح شات لأول مرة بس */
  private readonly _messagesByChat = signal<Map<string, Message[]>>(new Map());

  private readonly _selectedChatId = signal<string | null>(null);
  readonly selectedChatId = this._selectedChatId.asReadonly();

  private readonly _searchQuery = signal('');
  readonly searchQuery = this._searchQuery.asReadonly();

  private readonly _replyToMessageId = signal<string | null>(null);
  readonly replyToMessageId = this._replyToMessageId.asReadonly();

  private readonly _editingMessageId = signal<string | null>(null);

  private readonly _typingChatId = signal<string | null>(null);
  readonly typingChatId = this._typingChatId.asReadonly();

  private readonly _contacts = signal<User[]>([]);
  readonly contacts = this._contacts.asReadonly();

  readonly selectedChat = computed<Chat | undefined>(() =>
    this._chats().find((chat) => chat.id === this._selectedChatId()),
  );

  readonly filteredChats = computed<Chat[]>(() => {
    const query = this._searchQuery().trim().toLowerCase();
    if (!query) return this._chats();
    return this._chats().filter((chat) => {
      const nameMatch = chat.name.toLowerCase().includes(query);
      const lastMessageMatch = chat.lastMessage?.content.toLowerCase().includes(query) ?? false;
      return nameMatch || lastMessageMatch;
    });
  });

  readonly selectedChatMessages = computed<Message[]>(() => {
    const chatId = this._selectedChatId();
    if (!chatId) return [];
    return this._messagesByChat().get(chatId) ?? [];
  });

  readonly replyToMessage = computed<Message | undefined>(() => {
    const id = this._replyToMessageId();
    if (!id) return undefined;
    return this.selectedChatMessages().find((msg) => msg.id === id);
  });

  readonly editingMessage = computed<Message | undefined>(() => {
    const id = this._editingMessageId();
    if (!id) return undefined;
    return this.selectedChatMessages().find((msg) => msg.id === id);
  });

  readonly isOtherTypingInSelectedChat = computed(
    () => this._typingChatId() !== null && this._typingChatId() === this._selectedChatId(),
  );

  constructor() {
    // بنسمع لكل الأحداث اللحظية اللي جايالنا من SignalR، ونحدّث حالة
    // التطبيق تبعًا لها. ده المكان الوحيد في المشروع اللي بيتعامل مباشرة
    // مع الـ Events دي.
    this.signalR.messageReceived$.subscribe((message) => this.onMessageReceived(message));
    this.signalR.messageEdited$.subscribe((message) => this.onMessageEdited(message));
    this.signalR.messageDeleted$.subscribe(({ chatId, messageId }) => this.onMessageDeleted(chatId, messageId));
    this.signalR.chatCreated$.subscribe(() => this.loadChats());
    this.signalR.userTyping$.subscribe(({ chatId }) => this._typingChatId.set(chatId));
    this.signalR.userStoppedTyping$.subscribe(({ chatId }) => {
      if (this._typingChatId() === chatId) this._typingChatId.set(null);
    });
  }

  /** بتتنادى أول ما المستخدم يسجل دخول وتوصل SignalR - بتجيب قائمة المحادثات كلها */
  async loadChats(): Promise<void> {
    const chats = await firstValueFrom(this.http.get<Chat[]>(`${environment.apiUrl}/chats`));
    this._chats.set(chats);
  }

  async loadContacts(): Promise<void> {
    const contacts = await firstValueFrom(this.http.get<User[]>(`${environment.apiUrl}/users/contacts`));
    this._contacts.set(contacts);
  }

  async selectChat(chatId: string): Promise<void> {
    this._selectedChatId.set(chatId);
    this._replyToMessageId.set(null);
    this._editingMessageId.set(null);

    // لو الرسائل متجابتش قبل كده، هاتها من السيرفر
    if (!this._messagesByChat().has(chatId)) {
      const messages = await firstValueFrom(
        this.http.get<Message[]>(`${environment.apiUrl}/chats/${chatId}/messages`),
      );
      this._messagesByChat.update((map) => new Map(map).set(chatId, messages));
    }

    await this.markCurrentChatAsRead();
  }

  closeChat(): void {
    this._selectedChatId.set(null);
  }

  setSearchQuery(query: string): void {
    this._searchQuery.set(query);
  }

  setReplyTo(messageId: string): void {
    this._replyToMessageId.set(messageId);
  }

  cancelReply(): void {
    this._replyToMessageId.set(null);
  }

  startEditMessage(messageId: string): void {
    this._editingMessageId.set(messageId);
    this._replyToMessageId.set(null);
  }

  cancelEdit(): void {
    this._editingMessageId.set(null);
  }

  async saveEditedMessage(newContent: string): Promise<void> {
    const chatId = this._selectedChatId();
    const messageId = this._editingMessageId();
    if (!chatId || !messageId) return;

    try {
      await this.signalR.editMessage(chatId, messageId, newContent);
      this._editingMessageId.set(null);
      // التحديث الفعلي في الواجهة هيحصل لما event "MessageEdited" يوصل
    } catch (error) {
      console.error('فشل تعديل الرسالة:', error);
    }
  }

  async sendMessage(content: string): Promise<void> {
    const chatId = this._selectedChatId();
    if (!chatId || !content.trim()) return;

    const replyToMessageId = this._replyToMessageId();
    this._replyToMessageId.set(null);

    try {
      await this.signalR.sendMessage(chatId, MessageType.Text, content.trim(), replyToMessageId);
    } catch (error) {
      console.error('فشل إرسال الرسالة:', error);
    }
  }

  /**
   * إرسال صورة كـ Data URL. ملحوظة صراحة: ده حل مؤقت مناسب للتجربة والتطوير
   * بس - في تطبيق إنتاجي حقيقي، الصورة المفروض تتاح لـ Endpoint رفع ملفات
   * مخصص (زي Azure Blob Storage) ويترجعلك رابط بدل ما تتخزن كـ نص طويل
   * جوه عمود الرسالة نفسه في قاعدة البيانات.
   */
  async sendImageMessage(dataUrl: string): Promise<void> {
    const chatId = this._selectedChatId();
    if (!chatId) return;

    try {
      await this.signalR.sendMessage(chatId, MessageType.Image, dataUrl, null);
    } catch (error) {
      console.error('فشل إرسال الصورة:', error);
    }
  }

  async deleteMessage(messageId: string): Promise<void> {
    const chatId = this._selectedChatId();
    if (!chatId) return;

    try {
      await this.signalR.deleteMessage(chatId, messageId);
    } catch (error) {
      console.error('فشل حذف الرسالة:', error);
    }
  }

  async startChatWithUser(userId: string): Promise<void> {
    const chat = await firstValueFrom(
      this.http.post<Chat>(`${environment.apiUrl}/chats/direct`, { otherUserId: userId }),
    );
    await this.afterChatCreated(chat);
  }

  async createGroup(name: string, avatarUrl: string, memberIds: string[]): Promise<void> {
    const chat = await firstValueFrom(
      this.http.post<Chat>(`${environment.apiUrl}/chats/group`, { name, avatarUrl, memberIds }),
    );
    await this.afterChatCreated(chat);
  }

  async onTypingStart(): Promise<void> {
    const chatId = this._selectedChatId();
    if (chatId) await this.signalR.startTyping(chatId);
  }

  async onTypingStop(): Promise<void> {
    const chatId = this._selectedChatId();
    if (chatId) await this.signalR.stopTyping(chatId);
  }

  private async afterChatCreated(chat: Chat): Promise<void> {
    this._chats.update((chats) => (chats.some((c) => c.id === chat.id) ? chats : [chat, ...chats]));
    await this.signalR.joinChatGroup(chat.id);
    await this.selectChat(chat.id);
  }

  private async markCurrentChatAsRead(): Promise<void> {
    const chatId = this._selectedChatId();
    const messages = this.selectedChatMessages();
    const lastMessage = messages[messages.length - 1];
    if (!chatId || !lastMessage) return;

    try {
      await this.signalR.markAsRead(chatId, lastMessage.id);
      this._chats.update((chats) =>
        chats.map((chat) => (chat.id === chatId ? { ...chat, unreadCount: 0 } : chat)),
      );
    } catch (error) {
      console.error('فشل تحديث حالة القراءة:', error);
    }
  }

  private onMessageReceived(message: Message): void {
    this._messagesByChat.update((map) => {
      const next = new Map(map);
      const existing = next.get(message.chatId) ?? [];
      // احتياط: لو الرسالة دي وصلت قبل كده لأي سبب، متتكررش
      if (existing.some((m) => m.id === message.id)) return next;
      next.set(message.chatId, [...existing, message]);
      return next;
    });

    const isCurrentlyOpen = this._selectedChatId() === message.chatId;

    this._chats.update((chats) =>
      chats.map((chat) =>
        chat.id === message.chatId
          ? { ...chat, lastMessage: message, unreadCount: isCurrentlyOpen ? 0 : chat.unreadCount + 1 }
          : chat,
      ),
    );

    if (isCurrentlyOpen) void this.markCurrentChatAsRead();
  }

  private onMessageEdited(message: Message): void {
    this._messagesByChat.update((map) => {
      const next = new Map(map);
      const existing = next.get(message.chatId) ?? [];
      next.set(
        message.chatId,
        existing.map((m) => (m.id === message.id ? message : m)),
      );
      return next;
    });

    this._chats.update((chats) =>
      chats.map((chat) => (chat.lastMessage?.id === message.id ? { ...chat, lastMessage: message } : chat)),
    );
  }

  private onMessageDeleted(chatId: string, messageId: string): void {
    this._messagesByChat.update((map) => {
      const next = new Map(map);
      const existing = next.get(chatId) ?? [];
      next.set(
        chatId,
        existing.map((m) => (m.id === messageId ? { ...m, isDeleted: true, content: '' } : m)),
      );
      return next;
    });
  }

  /** بتُنادى لما المستخدم يسجل خروج - بنصفّر كل حالة الشات المحلية */
  resetState(): void {
    this._chats.set([]);
    this._messagesByChat.set(new Map());
    this._selectedChatId.set(null);
    this._contacts.set([]);
  }
}
