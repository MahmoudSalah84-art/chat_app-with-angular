import { Injectable, computed, signal } from '@angular/core';
import { Chat } from '../models/chat.model';
import { Message } from '../models/message.model';
import { User } from '../models/user.model';
import { MessageStatus, MessageType } from '../enums/message-status.enum';
import { CURRENT_USER, MOCK_CHATS, MOCK_MESSAGES, MOCK_USERS } from './mock-data';

/**
 * ChatService هو المسؤول عن كل حالة (State) الشات في التطبيق:
 * - قائمة المحادثات + البحث فيها
 * - المحادثة المختارة حاليًا
 * - رسائل كل محادثة (نص، صور، ردود على رسائل)
 *
 * دلوقتي بيقرأ من Mock Data، وبعدين لما نضيف Backend حقيقي،
 * هنغير جوه الدوال دي بس (نستبدلها بـ HTTP/WebSocket calls)
 * من غير ما نلمس أي Component بيستخدم الـ Service ده.
 */
@Injectable({
  providedIn: 'root',
})
export class ChatService {
  /** المستخدم الحالي (الشخص الداخل بحسابه) - Signal عشان يتحدث لما يعدل بروفايله */
  private readonly _currentUser = signal<User>(CURRENT_USER);
  readonly currentUser = this._currentUser.asReadonly();

  /** كل المحادثات - كـ Signal عشان أي تغيير يحدث تلقائي في الواجهة */
  private readonly _chats = signal<Chat[]>(MOCK_CHATS);
  readonly chats = this._chats.asReadonly();

  /** كل جهات الاتصال المتاحة لبدء محادثة جديدة معاها */
  readonly contacts = MOCK_USERS;

  /** كل الرسائل لكل المحادثات */
  private readonly _messages = signal<Message[]>(MOCK_MESSAGES);

  /** الـ id الخاص بالمحادثة المفتوحة حاليًا */
  private readonly _selectedChatId = signal<string | null>(null);
  readonly selectedChatId = this._selectedChatId.asReadonly();

  /** نص البحث الحالي في قائمة المحادثات */
  private readonly _searchQuery = signal<string>('');
  readonly searchQuery = this._searchQuery.asReadonly();

  /** الرسالة اللي المستخدم بيرد عليها دلوقتي (null لو مفيش) */
  private readonly _replyToMessageId = signal<string | null>(null);
  readonly replyToMessageId = this._replyToMessageId.asReadonly();

  /** الرسالة اللي المستخدم بيعدّلها دلوقتي (null لو مفيش تعديل جاري) */
  private readonly _editingMessageId = signal<string | null>(null);
  readonly editingMessageId = this._editingMessageId.asReadonly();

  /** الـ id بتاع المحادثة اللي الطرف التاني فيها "بيكتب دلوقتي" (محاكاة وهمية) */
  private readonly _typingChatId = signal<string | null>(null);
  readonly typingChatId = this._typingChatId.asReadonly();

  /** المحادثة المختارة كاملة (بيانات) - Computed Signal بيتحدث تلقائي */
  readonly selectedChat = computed<Chat | undefined>(() =>
    this._chats().find((chat) => chat.id === this._selectedChatId()),
  );

  /** المحادثات بعد تطبيق فلتر البحث (بالاسم أو بمحتوى آخر رسالة) */
  readonly filteredChats = computed<Chat[]>(() => {
    const query = this._searchQuery().trim().toLowerCase();
    if (!query) return this._chats();

    return this._chats().filter((chat) => {
      const nameMatch = chat.name.toLowerCase().includes(query);
      const lastMessageMatch = chat.lastMessage?.content.toLowerCase().includes(query) ?? false;
      return nameMatch || lastMessageMatch;
    });
  });

  /** رسائل المحادثة المختارة فقط، مرتبة زمنيًا */
  readonly selectedChatMessages = computed<Message[]>(() => {
    const chatId = this._selectedChatId();
    if (!chatId) return [];
    return this._messages()
      .filter((msg) => msg.chatId === chatId)
      .sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());
  });

  /** هل الطرف التاني في المحادثة المفتوحة دلوقتي بيكتب؟ */
  readonly isOtherTypingInSelectedChat = computed(
    () => this._typingChatId() !== null && this._typingChatId() === this._selectedChatId(),
  );
  readonly replyToMessage = computed<Message | undefined>(() => {
    const id = this._replyToMessageId();
    if (!id) return undefined;
    return this._messages().find((msg) => msg.id === id);
  });

  /** الرسالة الكاملة اللي بيتم تعديلها دلوقتي (لتعبئة الـ Input بمحتواها) */
  readonly editingMessage = computed<Message | undefined>(() => {
    const id = this._editingMessageId();
    if (!id) return undefined;
    return this._messages().find((msg) => msg.id === id);
  });

  /** فتح محادثة معينة */
  selectChat(chatId: string): void {
    this._selectedChatId.set(chatId);
    this._replyToMessageId.set(null); // نصفّر أي رد كان جاري لما نغيّر المحادثة
    this._editingMessageId.set(null); // ونصفّر أي تعديل كان جاري كمان
    this.markChatAsRead(chatId);
  }

  /** إغلاق المحادثة الحالية (يستخدم في زر الرجوع على الموبايل) */
  closeChat(): void {
    this._selectedChatId.set(null);
  }

  /**
   * بدء محادثة مع مستخدم معين (من قائمة "محادثة جديدة").
   * لو فيه محادثة فردية معاه بالفعل، بنفتحها بدل ما نعمل واحدة مكررة.
   */
  startChatWithUser(userId: string): void {
    const existingChat = this._chats().find(
      (chat) => !chat.isGroup && chat.participants.some((p) => p.id === userId),
    );

    if (existingChat) {
      this.selectChat(existingChat.id);
      return;
    }

    const contact = this.contacts.find((user) => user.id === userId);
    if (!contact) return;

    const newChat: Chat = {
      id: `chat-${Date.now()}`,
      isGroup: false,
      name: contact.name,
      avatarUrl: contact.avatarUrl,
      participants: [this.currentUser(), contact],
      unreadCount: 0,
    };

    this._chats.update((chats) => [newChat, ...chats]);
    this.selectChat(newChat.id);
  }

  /**
   * إنشاء مجموعة جديدة بأعضاء متعددين.
   * المستخدم الحالي بينضم تلقائيًا كأول عضو في المجموعة.
   */
  createGroup(name: string, avatarUrl: string, participantIds: string[]): void {
    const selectedContacts = this.contacts.filter((user) => participantIds.includes(user.id));
    if (!name.trim() || selectedContacts.length === 0) return;

    const newGroup: Chat = {
      id: `chat-${Date.now()}`,
      isGroup: true,
      name: name.trim(),
      avatarUrl,
      participants: [this.currentUser(), ...selectedContacts],
      unreadCount: 0,
    };

    this._chats.update((chats) => [newGroup, ...chats]);
    this.selectChat(newGroup.id);
  }
  setSearchQuery(query: string): void {
    this._searchQuery.set(query);
  }

  /** البحث عن رسالة معينة بالـ id - يستخدم لعرض معاينة الرد */
  findMessageById(messageId: string): Message | undefined {
    return this._messages().find((msg) => msg.id === messageId);
  }

  /** بدء الرد على رسالة معينة */
  setReplyTo(messageId: string): void {
    this._editingMessageId.set(null); // مينفعش ترد وتعدّل في نفس الوقت
    this._replyToMessageId.set(messageId);
  }

  /** إلغاء الرد الحالي */
  cancelReply(): void {
    this._replyToMessageId.set(null);
  }

  /** بدء تعديل رسالة معينة (لازم تكون رسالتك إنت، ومش محذوفة) */
  startEditMessage(messageId: string): void {
    const message = this.findMessageById(messageId);
    if (!message || message.senderId !== this.currentUser().id || message.isDeleted) return;

    this._replyToMessageId.set(null); // مينفعش تعدّل وترد في نفس الوقت
    this._editingMessageId.set(messageId);
  }

  /** إلغاء التعديل الحالي */
  cancelEdit(): void {
    this._editingMessageId.set(null);
  }

  /** حفظ التعديل على الرسالة اللي بيتم تعديلها دلوقتي */
  saveEditedMessage(newContent: string): void {
    const messageId = this._editingMessageId();
    const trimmed = newContent.trim();
    if (!messageId || !trimmed) return;

    this._messages.update((messages) =>
      messages.map((msg) => (msg.id === messageId ? { ...msg, content: trimmed, isEdited: true } : msg)),
    );

    // لو الرسالة دي هي آخر رسالة في المحادثة، نحدّث المعاينة في القائمة كمان
    this._chats.update((chats) =>
      chats.map((chat) => {
        if (chat.lastMessage?.id !== messageId) return chat;
        return { ...chat, lastMessage: { ...chat.lastMessage, content: trimmed, isEdited: true } };
      }),
    );

    this._editingMessageId.set(null);
  }

  /** حذف رسالة (لازم تكون رسالتك إنت) - بيستبدل المحتوى بنص "تم حذف الرسالة" */
  deleteMessage(messageId: string): void {
    const message = this.findMessageById(messageId);
    if (!message || message.senderId !== this.currentUser().id) return;

    this._messages.update((messages) =>
      messages.map((msg) =>
        msg.id === messageId ? { ...msg, isDeleted: true, content: '', replyToMessageId: undefined } : msg,
      ),
    );

    this._chats.update((chats) =>
      chats.map((chat) => {
        if (chat.lastMessage?.id !== messageId) return chat;
        return { ...chat, lastMessage: { ...chat.lastMessage, isDeleted: true, content: '' } };
      }),
    );

    if (this._editingMessageId() === messageId) this._editingMessageId.set(null);
  }

  /** تصفير عدد الرسائل الغير مقروءة لما تفتح الشات */
  private markChatAsRead(chatId: string): void {
    this._chats.update((chats) =>
      chats.map((chat) => (chat.id === chatId ? { ...chat, unreadCount: 0 } : chat)),
    );
  }

  /** إرسال رسالة نصية جديدة في المحادثة المفتوحة */
  sendMessage(content: string): void {
    const chatId = this._selectedChatId();
    if (!chatId || !content.trim()) return;

    this.pushMessage({
      chatId,
      type: MessageType.Text,
      content: content.trim(),
    });
  }

  /** إرسال صورة (كـ Data URL) في المحادثة المفتوحة */
  sendImageMessage(dataUrl: string): void {
    const chatId = this._selectedChatId();
    if (!chatId) return;

    this.pushMessage({
      chatId,
      type: MessageType.Image,
      content: dataUrl,
    });
  }

  /** دالة مشتركة لإنشاء أي رسالة (نص أو صورة) مع مراعاة الرد على رسالة لو موجود */
  private pushMessage(params: { chatId: string; type: MessageType; content: string }): void {
    const replyToMessageId = this._replyToMessageId() ?? undefined;

    const newMessage: Message = {
      id: `msg-${Date.now()}`,
      chatId: params.chatId,
      senderId: this.currentUser().id,
      type: params.type,
      content: params.content,
      timestamp: new Date(),
      status: MessageStatus.Sent,
      replyToMessageId,
    };

    this._messages.update((messages) => [...messages, newMessage]);

    this._chats.update((chats) =>
      chats.map((chat) => (chat.id === params.chatId ? { ...chat, lastMessage: newMessage } : chat)),
    );

    this._replyToMessageId.set(null); // نصفّر الرد بعد الإرسال

    setTimeout(() => this.updateMessageStatus(newMessage.id, MessageStatus.Delivered), 1500);

    this.simulateContactResponse(params.chatId);
  }

  /**
   * محاكاة بسيطة لشخص حقيقي بيرد: بعد شوية يبدأ "يكتب دلوقتي"،
   * وبعد فترة كتابة عشوائية يبعت رد جاهز ويوقف مؤشر الكتابة.
   * ده هيتشال تمامًا لما نربط SignalR حقيقي، ومحله هيبقى Event جاي من السيرفر.
   */
  private simulateContactResponse(chatId: string): void {
    const chat = this._chats().find((c) => c.id === chatId);
    const otherParticipant = chat?.participants.find((p) => p.id !== this.currentUser().id);
    if (!otherParticipant) return;

    const startTypingDelay = 900 + Math.random() * 700;
    const typingDuration = 1800 + Math.random() * 1400;

    setTimeout(() => {
      this._typingChatId.set(chatId);

      setTimeout(() => {
        this._typingChatId.set(null);
        this.receiveMockReply(chatId, otherParticipant);
      }, typingDuration);
    }, startTypingDelay);
  }

  /** إضافة رد تلقائي وهمي من الطرف التاني في المحادثة */
  private receiveMockReply(chatId: string, sender: User): void {
    const canPossibleReplies = ['تمام 👍', 'حاضر، هرد عليك بعدين', 'ماشي كده', '😄', 'أوكي فهمت', 'تمام يا صاحبي'];
    const content = canPossibleReplies[Math.floor(Math.random() * canPossibleReplies.length)];

    const replyMessage: Message = {
      id: `msg-${Date.now()}`,
      chatId,
      senderId: sender.id,
      type: MessageType.Text,
      content,
      timestamp: new Date(),
      status: MessageStatus.Delivered,
    };

    this._messages.update((messages) => [...messages, replyMessage]);

    this._chats.update((chats) =>
      chats.map((chat) => {
        if (chat.id !== chatId) return chat;
        // لو المحادثة دي مش مفتوحة دلوقتي، نزود عداد الرسائل الغير مقروءة
        const isCurrentlyOpen = this._selectedChatId() === chatId;
        return {
          ...chat,
          lastMessage: replyMessage,
          unreadCount: isCurrentlyOpen ? 0 : chat.unreadCount + 1,
        };
      }),
    );
  }

  private updateMessageStatus(messageId: string, status: MessageStatus): void {
    this._messages.update((messages) =>
      messages.map((msg) => (msg.id === messageId ? { ...msg, status } : msg)),
    );
  }

  
  /** تحديث بيانات المستخدم الحالي (الاسم، النبذة، الصورة، أو الإيميل) */
  updateProfile(changes: Partial<Pick<User, 'name' | 'about' | 'avatarUrl' | 'email'>>): void {
    this._currentUser.update((user) => ({ ...user, ...changes }));
  }
}
