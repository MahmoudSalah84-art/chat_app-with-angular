import { Component, inject } from '@angular/core';
import { ThemeService } from '../../core/services/theme.service';
import { UiService } from '../../core/services/ui.service';
import { Avatar } from '../../shared/components/avatar/avatar';
import { NewChat } from '../new-chat/new-chat';
import { NewGroup } from '../new-group/new-group';
import { ProfilePanel } from '../profile/profile-panel/profile-panel';
import { SettingsPanel } from '../profile/settings-panel/settings-panel';
import { ChatListItem } from './chat-list-item/chat-list-item';
import { ChatFacade } from 'app/core/facade/chat-facade.service';

@Component({
  selector: 'app-chat-list',
  imports: [ChatListItem, Avatar, ProfilePanel, SettingsPanel, NewChat, NewGroup],
  templateUrl: './chat-list.html',
  styleUrl: './chat-list.css',
})
export class ChatList {
  private readonly chatFacade = inject(ChatFacade);
  private readonly themeService = inject(ThemeService);
  private readonly uiService = inject(UiService);

  readonly chats = this.chatFacade.filteredChats;
  readonly selectedChat = this.chatFacade.selectedChat;
  readonly isDarkMode = this.themeService.isDarkMode;
  readonly searchQuery = this.chatFacade.searchQuery;
  readonly currentUser = this.chatFacade.currentUser;
  readonly sidebarView = this.uiService.sidebarView;

  onSelectChat(chatId: string): void {
    this.chatFacade.selectChat(chatId);
  }

  onSearchChange(value: string): void {
    this.chatFacade.setSearchQuery(value);
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
