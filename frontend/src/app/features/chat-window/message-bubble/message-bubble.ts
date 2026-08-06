import { Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Message } from '../../../core/models/message.model';
import { MessageStatus, MessageType } from '../../../core/enums/message-status.enum';
import { ChatService } from '../../../core/services/chat';
 
@Component({
  selector: 'app-message-bubble',
  imports: [FormsModule],
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
 
  /** هل الرسالة في وضع التعديل دلوقتي */
  readonly isEditing = signal(false);
  readonly editDraft = signal('');
 
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
 
  onEditClick(): void {
    this.editDraft.set(this.message().content);
    this.isEditing.set(true);
    this.closeMenu();
  }
 
  onSaveEdit(): void {
    this.chatService.editMessage(this.message().id, this.editDraft());
    this.isEditing.set(false);
  }
 
  onCancelEdit(): void {
    this.isEditing.set(false);
  }
 
  onDeleteClick(): void {
    const confirmed = window.confirm('تحذف الرسالة دي؟ الخطوة دي مش هترجع.');
    if (confirmed) {
      this.chatService.deleteMessage(this.message().id);
    }
    this.closeMenu();
  }
}