import { Component, computed, inject, signal } from '@angular/core';
import { ChatFacade } from '../../core/facade/chat-facade.service';
import { UiService } from '../../core/services/ui.service';
import { Avatar } from '../../shared/components/avatar/avatar';

@Component({
  selector: 'app-new-chat',
  imports: [Avatar],
  templateUrl: './new-chat.html',
  styleUrl: './new-chat.css',
})
export class NewChat{
  private readonly chatFacade = inject(ChatFacade);
  private readonly uiService = inject(UiService);

  private readonly _searchQuery = signal('');
  private readonly _isLoading = signal(true);

  readonly isLoading = this._isLoading.asReadonly();

  readonly filteredContacts = computed(() => {
    const query = this._searchQuery().trim().toLowerCase();
    const contacts = this.chatFacade.contacts();
    if (!query) return contacts;
    return contacts.filter((contact) => contact.name.toLowerCase().includes(query));
  });

  constructor() {
    this.chatFacade.loadContacts().finally(() => this._isLoading.set(false));
  }

  onSearchChange(value: string): void {
    this._searchQuery.set(value);
  }

  async onSelectContact(userId: string): Promise<void> {
    await this.chatFacade.startChatWithUser(userId);
    this.uiService.showChats();
  }

  onBack(): void {
    this.uiService.showChats();
  }

  onNewGroupClick(): void {
    this.uiService.showNewGroup();
  }
}
