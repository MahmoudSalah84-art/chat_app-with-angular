import { Component, ElementRef, OnDestroy, computed, effect, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatFacade } from '../../core/facade/chat-facade.service';
import { MessageType } from '../../core/enums/message-type.enum';
import { Avatar } from '../../shared/components/avatar/avatar';
import { MessageBubble } from './message-bubble/message-bubble';

 
@Component({
  selector: 'app-chat-window',
  imports: [FormsModule, Avatar, MessageBubble],
  templateUrl: './chat-window.html',
  styleUrl: './chat-window.css',
})

export class ChatWindow implements OnDestroy {
  private readonly chatFacade = inject(ChatFacade);
  private readonly scrollContainer = viewChild<ElementRef<HTMLDivElement>>('scrollContainer');
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');
  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');


  readonly selectedChat = this.chatFacade.selectedChat;
  readonly messages = this.chatFacade.selectedMessages;
  readonly replyToMessage = this.chatFacade.replyToMessage;
  readonly editingMessage = this.chatFacade.editingMessage;
  readonly isOtherTyping = this.chatFacade.isOtherTyping;
  readonly draftMessage = signal('');
  readonly typeEnum = MessageType;


  // search inside chat
  readonly isSearchOpen = signal(false);
  readonly inChatSearchQuery = signal('');
  readonly displayedMessages = computed(() => {
    const all = this.messages();
    const query = this.inChatSearchQuery().trim().toLowerCase();
    if (!query) return all;
    return all.filter((m) => !m.isDeleted && m.content.toLowerCase().includes(query));
  });

  // other participant in the chat (for 1-to-1 chats)
  readonly otherParticipant = computed(() => {
    const currentUserId = this.chatFacade.currentUser()?.id;
    const chat = this.selectedChat();
    if (!chat) return undefined;
    return chat.participants.find((p) => p.id !== currentUserId) ?? chat.participants[0];
  });

  // typing indicator management
  private typingTimeout: ReturnType<typeof setTimeout> | null = null;
  private isTypingActive = false;

  constructor() {
    // when messages or typing status changes, scroll to bottom
    effect(() => {
      this.messages();
      this.isOtherTyping();
      queueMicrotask(() => this.scrollToBottom()); //for scrolling after the view has updated because DOM updates are async
    });

     // when the editing message changes, we update the draft message
    effect(() => {
      const editMsg = this.editingMessage();
      if (editMsg) {
        this.draftMessage.set(editMsg.content);
      }
    });

    // when the selected chat changes, we reset the typing indicator and close the search
    effect(() => {
      this.selectedChat();
      this.stopTyping();
      this.isSearchOpen.set(false);
      this.inChatSearchQuery.set('');
    });
  }


  ngOnDestroy(): void {
    this.stopTyping();
  }

 
  private scrollToBottom(): void {
   const el = this.scrollContainer()?.nativeElement;
   if (el) el.scrollTop = el.scrollHeight;
  }

  // for handling typing indicator and sending messages
  onInputChange(val: string): void {
    this.draftMessage.set(val);

    if (this.editingMessage()) return;

    if (val.trim()) {
      if (!this.isTypingActive) {
        this.isTypingActive = true;
        void this.chatFacade.onTypingStart();
      }
      if (this.typingTimeout) clearTimeout(this.typingTimeout);
      this.typingTimeout = setTimeout(() => {
        this.stopTyping();
      }, 2000);
    } else { // if the input is empty, we stop typing ( user deleted all text)
      this.stopTyping();
    }
  }

  private stopTyping(): void {
    if (this.isTypingActive) {
      this.isTypingActive = false;
      if (this.typingTimeout) {
        clearTimeout(this.typingTimeout);
        this.typingTimeout = null;
      }
      void this.chatFacade.onTypingStop();
    }
  }

  async onSend(): Promise<void> {
    const text = this.draftMessage().trim();
    if (!text) return;
    this.stopTyping();
    if (this.editingMessage()) {
      await this.chatFacade.saveEdit(text);
      this.draftMessage.set('');
    } else {
      await this.chatFacade.sendMessage(text);
      this.draftMessage.set('');
    }
  }


  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      void this.onSend();
    }
  }

  onBack(): void {
    this.stopTyping();
    this.chatFacade.closeChat();
  }
  onCancelReply(): void {
    this.chatFacade.cancelReply();
  }
  onCancelEdit(): void {
    this.chatFacade.cancelEdit();
    this.draftMessage.set('');
  }



  // search inside chat methods
  toggleSearch(): void {
    this.isSearchOpen.update((v) => !v);
    if (!this.isSearchOpen()) {
      this.inChatSearchQuery.set('');
    } else {
      setTimeout(() => this.searchInput()?.nativeElement.focus(), 50);
    }
  }
  
  closeSearch(): void {
    this.isSearchOpen.set(false);
    this.inChatSearchQuery.set('');
  }
  onSearchQueryChange(val: string): void {
    this.inChatSearchQuery.set(val);
  }

    formatLastSeen(lastSeenAt?: string | null): string {
    if (!lastSeenAt) return 'غير متصل';
    try {
      const date = new Date(lastSeenAt);
      if (isNaN(date.getTime())) return 'غير متصل';
      const now = new Date();
      const isToday = date.toDateString() === now.toDateString();
      const timeStr = date.toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
      if (isToday) return `آخر ظهور اليوم ${timeStr}`;
      return `آخر ظهور ${date.toLocaleDateString('ar-EG')} ${timeStr}`;
    } catch {
      return 'غير متصل';
    }
  }
  onAttachClick(): void {
    this.fileInput()?.nativeElement.click();
  }



   onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !file.type.startsWith('image/')) return;
    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        void this.chatFacade.sendImageMessage(reader.result);
      }
    };
    reader.readAsDataURL(file);
    input.value = '';
  }
}



