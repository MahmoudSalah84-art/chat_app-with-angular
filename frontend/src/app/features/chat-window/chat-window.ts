import { Component, ElementRef, computed, effect, inject, signal, untracked, viewChild } from '@angular/core';
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
  host: { '(document:keydown.escape)': 'onEscape()' },
})
export class ChatWindow {
  private readonly chatFacade = inject(ChatFacade);
  private readonly scrollContainer = viewChild<ElementRef<HTMLDivElement>>('scrollContainer');
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');
  private readonly messageInput = viewChild<ElementRef<HTMLInputElement>>('messageInput');
 
  readonly selectedChat = this.chatFacade.selectedChat;
  readonly messages = this.chatFacade.selectedMessages;
  readonly replyToMessage = this.chatFacade.replyToMessage;
  readonly editingMessage = this.chatFacade.editingMessage;
  readonly isOtherTyping = this.chatFacade.isOtherTyping;
 
  readonly draftMessage = signal('');
  readonly typeEnum = MessageType;
 
  // ─── الرسالة المتحددة (شريط الإجراءات اللي فوق) ───
  readonly selectedMessage = this.chatFacade.selectedMessage;
 
  private readonly myId = computed(() => this.chatFacade.currentUser()?.id);
 
  /** التعديل لرسايلك النصية بس */
  readonly canEditSelected = computed(() => {
    const m = this.selectedMessage();
    return !!m && m.senderId === this.myId() && m.type === MessageType.Text;
  });
 
  /** الحذف لرسايلك بس */
  readonly canDeleteSelected = computed(() => {
    const m = this.selectedMessage();
    return !!m && m.senderId === this.myId();
  });
 
  constructor() {
    // كل ما الرسائل تتغير (رسالة جديدة أو تغيير محادثة) أو يظهر مؤشر الكتابة، ننزل لآخر الشات تلقائيًا
    effect(() => {
      this.messages(); // للـ tracking بس
      this.isOtherTyping(); // للـ tracking بس
      queueMicrotask(() => this.scrollToBottom());
    });
 
    // لما يبدأ تعديل: نحط نص الرسالة في الـ input ونركّز عليه. ولما التعديل يخلص (حفظ/إلغاء/تغيير الشات) نفضّي الـ input.
    // بنتابع الـ id مش الرسالة نفسها، عشان تفاعل جديد على الرسالة ميكتبش فوق اللي بتكتبه.
    let wasEditing = false;
    effect(() => {
      const id = this.chatFacade.editingMessageId();
      if (id) {
        const msg = untracked(() => this.chatFacade.editingMessage());
        if (msg) {
          this.draftMessage.set(msg.content);
          wasEditing = true;
          queueMicrotask(() => this.focusInput());
        }
      } else if (wasEditing) {
        wasEditing = false;
        this.draftMessage.set('');
      }
    });
  }
 
  private scrollToBottom(): void {
    const el = this.scrollContainer()?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }
 
  private focusInput(): void {
    this.messageInput()?.nativeElement.focus();
  }
 
  onSend(): void {
    const text = this.draftMessage().trim();
    if (!text) return;
 
    const editing = this.editingMessage();
    if (editing) {
      if (text === editing.content) this.chatFacade.cancelEdit();
      else void this.chatFacade.saveEdit(text);
      return; // الـ effect هيفضّي الـ input لما التعديل يخلص
    }
 
    this.chatFacade.sendMessage(text);
    this.draftMessage.set('');
  }
 
  // send message on pressing Enter (without Shift)
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSend();
    } else if (event.key === 'Escape' && this.editingMessage()) {
      this.onCancelEdit();
    }
  }
 
  /** يستخدم في زر الرجوع اللي بيظهر بس على الموبايل */
  onBack(): void {
    this.chatFacade.closeChat();
  }
 
  onCancelReply(): void {
    this.chatFacade.cancelReply();
  }
 
  onCancelEdit(): void {
    this.chatFacade.cancelEdit();
  }
 
  // ─── Selection actions ───
  /** ضغطة على أي مكان فاضي في الشات بتلغي التحديد */
  onMessagesAreaClick(): void {
    this.chatFacade.clearSelectionFromTap();
  }
 
  onClearSelection(): void {
    this.chatFacade.clearSelection();
  }
 
  onEscape(): void {
    if (this.selectedMessage()) this.chatFacade.clearSelection();
  }
 
  onReplySelected(): void {
    const m = this.selectedMessage();
    if (!m) return;
    this.chatFacade.setReplyTo(m.id);
    this.chatFacade.clearSelection();
    this.focusInput();
  }
 
  onEditSelected(): void {
    const m = this.selectedMessage();
    if (!m || !this.canEditSelected()) return;
    this.chatFacade.startEdit(m.id);
    this.chatFacade.clearSelection();
  }
 
  async onDeleteSelected(): Promise<void> {
    const m = this.selectedMessage();
    if (!m || !this.canDeleteSelected()) return;
    this.chatFacade.clearSelection();
 
    if (window.confirm('تحذف الرسالة دي؟ الخطوة دي مش هترجع.')) {
      await this.chatFacade.deleteMessage(m.id);
    }
  }
 
  /** يفتح نافذة اختيار الملف من الجهاز (بيدوس على الـ input المخفي) */
  onAttachClick(): void {
    this.fileInput()?.nativeElement.click();
  }
 
  /** يقرأ الصورة المختارة ويحولها لـ Data URL عشان نعرضها فورًا (Mock بدون رفع حقيقي) */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !file.type.startsWith('image/')) return;
 
    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        this.chatFacade.sendImageMessage(reader.result);
      }
    };
    reader.readAsDataURL(file);
 
    input.value = ''; // نفضّي القيمة عشان تقدر تختار نفس الصورة تاني لو حبيت
  }
}