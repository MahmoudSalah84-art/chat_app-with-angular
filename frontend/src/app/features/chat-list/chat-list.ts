import { Component, inject } from '@angular/core';
import { ChatService } from '../../core/services/chat';
import { ThemeService } from '../../core/services/theme';
import { ChatListItem } from './chat-list-item/chat-list-item';

@Component({
  selector: 'app-chat-list',
  imports: [ChatListItem],
  templateUrl: './chat-list.html',
  styleUrl: './chat-list.css',
})
export class ChatList {
  private readonly chatService = inject(ChatService);
  private readonly themeService = inject(ThemeService);

  readonly chats = this.chatService.filteredChats;
  readonly selectedChatId = this.chatService.selectedChatId;
  readonly isDarkMode = this.themeService.isDarkMode;
  readonly searchQuery = this.chatService.searchQuery;

  onSelectChat(chatId: string): void {
    this.chatService.selectChat(chatId);
  }

  onSearchChange(value: string): void {
    this.chatService.setSearchQuery(value);
  }

  onToggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
