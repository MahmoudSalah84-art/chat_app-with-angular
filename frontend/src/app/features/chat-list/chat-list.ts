import { Component, inject } from '@angular/core';
import { ChatService } from '../../core/services/chat';
import { ThemeService } from '../../core/services/theme';
import { UiService } from '../../core/services/ui';
import { Avatar } from '../../shared/components/avatar/avatar';
import { NewChat } from '../new-chat/new-chat';
import { NewGroup } from '../new-group/new-group';
import { ProfilePanel } from '../profile/profile-panel/profile-panel';
import { SettingsPanel } from '../profile/settings-panel/settings-panel';
import { ChatListItem } from './chat-list-item/chat-list-item';

@Component({
  selector: 'app-chat-list',
  imports: [ChatListItem, Avatar, ProfilePanel, SettingsPanel, NewChat, NewGroup],
  templateUrl: './chat-list.html',
  styleUrl: './chat-list.css',
})
export class ChatList {
  private readonly chatService = inject(ChatService);
  private readonly themeService = inject(ThemeService);
  private readonly uiService = inject(UiService);

  readonly chats = this.chatService.filteredChats;
  readonly selectedChatId = this.chatService.selectedChatId;
  readonly isDarkMode = this.themeService.isDarkMode;
  readonly searchQuery = this.chatService.searchQuery;
  readonly currentUser = this.chatService.currentUser;
  readonly sidebarView = this.uiService.sidebarView;

  onSelectChat(chatId: string): void {
    this.chatService.selectChat(chatId);
  }

  onSearchChange(value: string): void {
    this.chatService.setSearchQuery(value);
  }

  onToggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onOpenProfile(): void {
    this.uiService.showProfile();
  }

  onOpenSettings(): void {
    this.uiService.showSettings();
  }

  onOpenNewChat(): void {
    this.uiService.showNewChat();
  }
}
