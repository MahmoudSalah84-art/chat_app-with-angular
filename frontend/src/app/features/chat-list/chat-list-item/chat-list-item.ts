import { Component, computed, inject, input, output } from '@angular/core';
import { Chat } from '../../../core/models/chat.model';
import { ChatFacade } from '../../../core/facade/chat-facade.service';
  import { MessageType } from '../../../core/enums/message-type.enum';
import { Avatar } from '../../../shared/components/avatar/avatar';

@Component({
  selector: 'app-chat-list-item',
  imports: [Avatar],
  templateUrl: './chat-list-item.html',
  styleUrl: './chat-list-item.css',
})
export class ChatListItem {
  private readonly chatFacade = inject(ChatFacade);

  readonly chat = input.required<Chat>();
  readonly isSelected = input<boolean>(false);
  readonly selected = output<string>();

  readonly isTyping = computed(() => this.chatFacade.typingChatId() === this.chat().id);

  readonly lastMessagePreview = computed(() => {
    const msg = this.chat().lastMessage;
    if (msg?.type === MessageType.Image) return '📷 صورة';
    return msg ? msg.content : 'لا توجد رسائل بعد';
  });

  readonly lastMessageTime = computed(() => {
    const msg = this.chat().lastMessage;
    if (!msg) return '';
    return new Date(msg.sentAt).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' });
  });

  onClick(): void {
    this.selected.emit(this.chat().id);
  }
}
