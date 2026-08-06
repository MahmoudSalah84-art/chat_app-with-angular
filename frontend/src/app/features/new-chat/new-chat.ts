import { Component, computed, inject, signal } from '@angular/core';
import { ChatService } from '../../core/services/chat';
import { UiService } from '../../core/services/ui';
import { Avatar } from '../../shared/components/avatar/avatar';


/**
 * بانل يعرض جهات الاتصال المتاحة لبدء محادثة جديدة معاها.
 * بيستبدل قائمة المحادثات مؤقتًا، وفيه بحث بسيط بالاسم.
 */
@Component({
  selector: 'app-new-chat',
  imports: [Avatar],
  templateUrl: './new-chat.html',
  styleUrl: './new-chat.css',
})
export class NewChat {
  private readonly chatService = inject(ChatService);
  private readonly uiService = inject(UiService);

  private readonly _searchQuery = signal('');

  /** جهات الاتصال بعد تطبيق البحث بالاسم */
  readonly filteredContacts = computed(() => {
    const query = this._searchQuery().trim().toLowerCase();
    const contacts = this.chatService.contacts;
    if (!query) return contacts;
    return contacts.filter((contact) => contact.name.toLowerCase().includes(query));
  });

  onSearchChange(value: string): void {
    this._searchQuery.set(value);
  }

  onSelectContact(userId: string): void {
    this.chatService.startChatWithUser(userId);
    this.uiService.showChats();
  }

  onBack(): void {
    this.uiService.showChats();
  }
}
