import { Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat';
import { MessageType } from '../../core/enums/message-type.enum';
import { Avatar } from '../../shared/components/avatar/avatar';
import { MessageBubble } from './message-bubble/message-bubble';

@Component({
  selector: 'app-chat-window',
  imports: [FormsModule, Avatar, MessageBubble],
  templateUrl: './chat-window.html',
  styleUrl: './chat-window.css',
})
export class ChatWindow {
  private readonly chatService = inject(ChatService);
  private readonly scrollContainer = viewChild<ElementRef<HTMLDivElement>>('scrollContainer');
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');


  readonly selectedChat = this.chatService.selectedChat;
  readonly messages = this.chatService.selectedChatMessages;
  readonly replyToMessage = this.chatService.replyToMessage;
  readonly isOtherTyping = this.chatService.isOtherTypingInSelectedChat;
 
  readonly draftMessage = signal('');
  readonly typeEnum = MessageType;

 constructor() {
    // كل ما الرسائل تتغير (رسالة جديدة أو تغيير محادثة) أو يظهر مؤشر الكتابة، ننزل لآخر الشات تلقائيًا
    effect(() => {
      this.messages(); // للـ tracking بس
      this.isOtherTyping(); // للـ tracking بس
      queueMicrotask(() => this.scrollToBottom());
    });
  }

  private scrollToBottom(): void {
    const el = this.scrollContainer()?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
  }

  onSend(): void {
    const text = this.draftMessage().trim();
    if (!text) return;
    this.chatService.sendMessage(text);
    this.draftMessage.set('');
  }

  // send message on pressing Enter (without Shift)
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSend();
    }
  }

  /** يستخدم في زر الرجوع اللي بيظهر بس على الموبايل */
  onBack(): void {
    this.chatService.closeChat();
  }
 
  onCancelReply(): void {
    this.chatService.cancelReply();
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
        this.chatService.sendImageMessage(reader.result);
      }
    };
    reader.readAsDataURL(file);
 
    input.value = ''; // نفضّي القيمة عشان تقدر تختار نفس الصورة تاني لو حبيت
  }
}
