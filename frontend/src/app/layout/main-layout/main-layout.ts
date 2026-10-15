import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { ChatFacade } from '../../core/facade/chat-facade.service';
import { PushService } from '../../core/services/push.service';
import { ChatList } from '../../features/chat-list/chat-list';
import { ChatWindow } from '../../features/chat-window/chat-window';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [ChatList, ChatWindow],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
})
export class MainLayout implements OnInit {
  private readonly chatFacade = inject(ChatFacade);
  private readonly push = inject(PushService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly hasSelectedChat = () => this.chatFacade.selectedChat() != null;
  readonly showPushPrompt = this.push.shouldPrompt;
  readonly isPushBusy = this.push.isBusy;


  ngOnInit(): void {
    void this.push.init();

    this.push.openChat$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((chatId) => void this.openChat(chatId));

    const chatId = this.route.snapshot.queryParamMap.get('chatId');
    if (chatId) {
      void this.openChat(chatId);
      void this.router.navigate([], { queryParams: { chatId: null }, queryParamsHandling: 'merge', replaceUrl: true });
    }
  }

  onEnablePush(): void {
    void this.push.enable();
  }

  onDismissPush(): void {
    this.push.dismissPrompt();
  }

  private async openChat(chatId: string): Promise<void> {
    if (this.chatFacade.chats().length === 0) await this.chatFacade.loadChats();
    await this.chatFacade.selectChat(chatId);
  }
}