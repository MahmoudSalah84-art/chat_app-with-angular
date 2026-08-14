import { Component, computed, inject, signal } from '@angular/core';
import { ChatService } from '../../core/services/chat';
import { UiService } from '../../core/services/ui';
import { Avatar } from '../../shared/components/avatar/avatar';

@Component({
  selector: 'app-new-chat',
  imports: [Avatar],
  templateUrl: './new-chat.html',
  styleUrl: './new-chat.css',
})
export class NewChat{
  private readonly chatService = inject(ChatService);
  private readonly uiService = inject(UiService);

  private readonly _searchQuery = signal('');
  private readonly _isLoading = signal(true);

  readonly isLoading = this._isLoading.asReadonly();

  readonly filteredContacts = computed(() => {
    const query = this._searchQuery().trim().toLowerCase();
    const contacts = this.chatService.contacts();
    if (!query) return contacts;
    return contacts.filter((contact) => contact.name.toLowerCase().includes(query));
  });

  constructor() {
    this.chatService.loadContacts().finally(() => this._isLoading.set(false));
  }

  onSearchChange(value: string): void {
    this._searchQuery.set(value);
  }

  async onSelectContact(userId: string): Promise<void> {
    await this.chatService.startChatWithUser(userId);
    this.uiService.showChats();
  }

  onBack(): void {
    this.uiService.showChats();
  }

  onNewGroupClick(): void {
    this.uiService.showNewGroup();
  }
}
