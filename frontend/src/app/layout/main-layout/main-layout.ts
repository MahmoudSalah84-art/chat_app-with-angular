import { Component, inject } from '@angular/core';
import { ChatFacade } from '../../core/facade/chat-facade.service';
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
  private readonly chatFacade = inject(ChatFacade);
 
  readonly hasSelectedChat = () => this.chatFacade.selectedChat() !== null;
}