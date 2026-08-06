import { Component, computed, input, output } from '@angular/core';
import { Chat } from '../../../core/models/chat.model';
import { Avatar } from '../../../shared/components/avatar/avatar';

@Component({
  selector: 'app-chat-list-item',
  imports: [Avatar],
  templateUrl: './chat-list-item.html',
  styleUrl: './chat-list-item.css',
})
export class ChatListItem {
  readonly chat = input.required<Chat>();
  readonly isSelected = input<boolean>(false);
  readonly selected = output<string>();

  /** نص آخر رسالة يظهر تحت الاسم */
  readonly lastMessagePreview = computed(() => {
    const msg = this.chat().lastMessage;
    return msg ? msg.content : 'لا توجد رسائل بعد';
  });

  /** وقت آخر رسالة بصيغة مختصرة (زي واتساب: HH:MM) */
  readonly lastMessageTime = computed(() => {
    const msg = this.chat().lastMessage;
    if (!msg) return '';
    return msg.timestamp.toLocaleTimeString('ar-EG', {
      hour: '2-digit',
      minute: '2-digit',
    });
  });

  onClick(): void {
    this.selected.emit(this.chat().id);
  }
}
