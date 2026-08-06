import { Component, inject } from '@angular/core';
import { ChatService } from '../../core/services/chat';
import { ChatList } from '../../features/chat-list/chat-list';
import { ChatWindow } from '../../features/chat-window/chat-window';
 
@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [ChatList, ChatWindow],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
})
export class MainLayout {
  private readonly chatService = inject(ChatService);
 
  readonly hasSelectedChat = () => this.chatService.selectedChatId() !== null;
}