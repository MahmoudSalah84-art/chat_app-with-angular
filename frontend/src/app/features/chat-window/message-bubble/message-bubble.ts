import { Component, computed, inject, input, signal } from '@angular/core';
import { Message } from '../../../core/models/message.model';
import { MessageStatus, MessageType } from '../../../core/enums/message-status.enum';
import { ChatService } from '../../../core/services/chat';

@Component({
  selector: 'app-message-bubble',
  imports: [],
  templateUrl: './message-bubble.html',
  styleUrl: './message-bubble.css',
})
export class MessageBubble {
  private readonly chatService = inject(ChatService);

  readonly message = input.required<Message>();

  /** هل الرسالة دي مبعوتة من المستخدم الحالي (تظهر يمين) */
  readonly isOwnMessage = computed(
    () => this.message().senderId === this.chatService.currentUser().id,
  );

  /** لو الرسالة دي رد على رسالة تانية، نجيب الرسالة الأصلية عشان نعرض معاينة منها */
  readonly repliedMessage = computed(() => {
    const replyId = this.message().replyToMessageId;
    if (!replyId) return undefined;
    return this.chatService.findMessageById(replyId);
  });

  readonly time = computed(() =>
    this.message().timestamp.toLocaleTimeString('ar-EG', {
      hour: '2-digit',
      minute: '2-digit',
    }),
  );

  readonly statusEnum = MessageStatus;
  readonly typeEnum = MessageType;

  /** هل قائمة الخيارات (⋮) مفتوحة دلوقتي */
  readonly isMenuOpen = signal(false);

  /** التعديل والحذف مسموحين بس لرسائلي أنا، ولو مش صورة أو محذوفة بالفعل */
  readonly canEdit = computed(
    () => this.isOwnMessage() && this.message().type === this.typeEnum.Text && !this.message().isDeleted,
  );
  readonly canDelete = computed(() => this.isOwnMessage() && !this.message().isDeleted);

  toggleMenu(): void {
    this.isMenuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  onReplyClick(): void {
    this.chatService.setReplyTo(this.message().id);
    this.closeMenu();
  }

  /** بيحط الرسالة في وضع التعديل - النص هيظهر جاهز في شريط الإدخال الرئيسي تحت */
  onEditClick(): void {
    this.chatService.startEditMessage(this.message().id);
    this.closeMenu();
  }

  onDeleteClick(): void {
    const confirmed = window.confirm('تحذف الرسالة دي؟ الخطوة دي مش هترجع.');
    if (confirmed) {
      this.chatService.deleteMessage(this.message().id);
    }
    this.closeMenu();
  }
}
