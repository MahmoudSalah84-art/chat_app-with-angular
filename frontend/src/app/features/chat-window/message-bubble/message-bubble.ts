import { DestroyRef, Component, computed, inject, input, signal } from '@angular/core';
import { Message } from '../../../core/models/message.model';
import { MessageType } from '../../../core/enums/message-type.enum';
import { AuthService } from '../../../core/services/auth.service';
import { ChatFacade } from '../../../core/facade/chat-facade.service';
import { environment } from '../../../../environments/environment';
import { formatBytes } from '../../../core/utils/attachment-rules';
import { getMessageStatus } from '../../../core/utils/message-status';
import { MessageStatus } from '../../../core/enums/message-status.enum';
import { REACTION_EMOJIS } from '../../../core/constants/reactions';

const LONG_PRESS_MS = 450;
const MOVE_TOLERANCE_PX = 10;


@Component({
  selector: 'app-message-bubble',
  imports: [],
  templateUrl: './message-bubble.html',
  styleUrl: './message-bubble.css',
})
export class MessageBubble {
  private readonly chatFacade = inject(ChatFacade);
  private readonly authService = inject(AuthService);

  readonly message = input.required<Message>();
  readonly attachment = computed(() => this.message().attachment ?? null);
  readonly imageUrl = computed(() => {
    const url = this.attachment()?.url ?? this.message().content;
    return url.includes('/image/upload/')
      ? url.replace('/image/upload/', '/image/upload/w_600,q_auto,f_auto/')
      : url;
  });
  readonly sizeLabel = computed(() => formatBytes(this.attachment()?.sizeInBytes ?? 0));
  readonly isOwnMessage = computed(() => this.message().senderId === this.authService.currentUser()?.id);
  readonly repliedMessage = computed(() => {
    const replyId = this.message().replyToMessageId;
    if (!replyId) return undefined;
    return this.chatFacade.selectedMessages().find((m) => m.id === replyId);
  });
  readonly time = computed(() =>
    new Date(this.message().sentAt).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' }),
  );
  readonly typeEnum = MessageType;
  readonly isMenuOpen = signal(false);
  readonly isLightboxOpen = signal(false);
  readonly canEdit = computed(
    () => this.isOwnMessage() && this.message().type === this.typeEnum.Text && !this.message().isDeleted,
  );
  readonly canDelete = computed(() => this.isOwnMessage() && !this.message().isDeleted);
  readonly mediaUrl = computed(() => this.resolveMediaUrl(this.message().attachment?.url ?? this.message().content));
  readonly documentData = computed(() => {
    if (this.message().type !== this.typeEnum.File) return null;
    try {
      const parsed = JSON.parse(this.message().content);
      return {
        url: this.resolveMediaUrl(parsed.url || ''),
        fileName: parsed.fileName || 'مستند',
        fileSize: parsed.fileSize ? this.formatFileSize(parsed.fileSize) : '',
      };
    } catch {
      return {
        url: this.resolveMediaUrl(this.message().content),
        fileName: 'مستند',
        fileSize: '',
      };
    }
  });

  readonly statusEnum = MessageStatus;
  readonly status = computed(() =>
    getMessageStatus(this.message(), this.chatFacade.selectedChat(), this.authService.currentUser()?.id ?? ''),
  );

  toggleMenu(): void {
    this.isMenuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  openLightbox(): void {
    this.isLightboxOpen.set(true);
  }

  closeLightbox(): void {
    this.isLightboxOpen.set(false);
  }

  onReplyClick(): void {
    this.chatFacade.setReplyTo(this.message().id);
    this.closeMenu();
  }

  onEditClick(): void {
    this.chatFacade.startEdit(this.message().id);
    this.closeMenu();
  }

  onDeleteClick(): void {
    const confirmed = window.confirm('تحذف الرسالة دي؟ الخطوة دي مش هترجع.');
    if (confirmed) {
      void this.chatFacade.deleteMessage(this.message().id);
    }
    this.closeMenu();
  }

  resolveMediaUrl(url: string): string {
    if (!url) return '';
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    return `${baseUrl}${url.startsWith('/') ? '' : '/'}${url}`;
  }

  formatFileSize(bytes: number): string {
    if (!bytes || bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }

  formatRepliedPreview(msg: Message): string {
    switch (msg.type) {
      case this.typeEnum.Image:
        return '📷 صورة';
      case this.typeEnum.Video:
        return '🎥 فيديو';
      case this.typeEnum.Audio:
        return '🎵 مقطع صوتي';
      case this.typeEnum.File:
        return '📄 ملف';
      default:
        return msg.content;
    }
  }

  readonly reactionEmojis = REACTION_EMOJIS;
  readonly isPickerOpen = signal(false);

  readonly myReaction = computed(() => {
    const me = this.authService.currentUser()?.id;
    return this.message().reactions?.find((r) => r.userId === me)?.emoji ?? null;
  });

  /** تجميع للعرض: { emoji, count, mine, names } مرتبة بالأكتر استخدامًا */
  readonly reactionGroups = computed(() => {
    const me = this.authService.currentUser()?.id;
    const names = new Map(
      (this.chatFacade.selectedChat()?.participants ?? []).map((p) => [p.id, p.id === me ? 'إنت' : p.name]),
    );

    const groups = new Map<string, { emoji: string; count: number; mine: boolean; names: string[] }>();
    for (const r of this.message().reactions ?? []) {
      const g = groups.get(r.emoji) ?? { emoji: r.emoji, count: 0, mine: false, names: [] };
      g.count++;
      g.mine ||= r.userId === me;
      g.names.push(names.get(r.userId) ?? '');
      groups.set(r.emoji, g);
    }
    return [...groups.values()].sort((a, b) => b.count - a.count);
  });

  //togglePicker(): void { this.isPickerOpen.update((v) => !v); }
  //closePicker(): void { this.isPickerOpen.set(false); }

  onReact(emoji: string): void {
    void this.chatFacade.reactToMessage(this.message().id, emoji);
    this.closePicker();
  }

  // ─── Long press / picker logic ──_______________________________________________________________
  private readonly destroyRef = inject(DestroyRef);
  private longPressTimer: ReturnType<typeof setTimeout> | null = null;
  private pressStart: { x: number; y: number } | null = null;
  private longPressFired = false;
  private pickerOpenedAt = 0;

  readonly pickerBelow = signal(false); // if the picker should open below the bubble (if it's near the top of the screen)

  constructor() {
    this.destroyRef.onDestroy(() => this.clearLongPress());
    
  }

  openPicker(anchor: HTMLElement): void {
    // الـ scroll container بيقص أي حاجة فوقه، فلو الرسالة قريبة من الحافة نفتح تحت
    this.pickerBelow.set(anchor.getBoundingClientRect().top < 120);
    this.pickerOpenedAt = Date.now();
    this.isPickerOpen.set(true);
  }

  togglePicker(anchor: HTMLElement): void {
    this.isPickerOpen() ? this.closePicker() : this.openPicker(anchor);
  }

  closePicker(): void {
    this.isPickerOpen.set(false);
  }

  /** تجاهل أي "click" بييجي بعد رفع الصباع مباشرة من الضغطة الطويلة */
  onBackdropClick(): void {
    if (Date.now() - this.pickerOpenedAt < 350) return;
    this.closePicker();
  }

  // ─── Long press ───
  onPointerDown(event: PointerEvent, bubble: HTMLElement): void {
    this.longPressFired = false; // أي تفاعل جديد يصفّر الحالة القديمة

    if (event.pointerType === 'mouse') return;            // الماوس له hover
    if (this.message().isDeleted || this.isPickerOpen()) return;
    if ((event.target as HTMLElement).closest('button, audio, video')) return; // chips / أزرار / controls

    this.clearLongPress();
    this.pressStart = { x: event.clientX, y: event.clientY };
    this.longPressTimer = setTimeout(() => {
      this.longPressFired = true;
      this.longPressTimer = null;
      navigator.vibrate?.(15); // haptic خفيف (Android بس، iOS بيتجاهله)
      this.openPicker(bubble);
    }, LONG_PRESS_MS);
  }

  onPointerMove(event: PointerEvent): void {
    if (!this.longPressTimer || !this.pressStart) return;
    const moved = Math.hypot(event.clientX - this.pressStart.x, event.clientY - this.pressStart.y);
    if (moved > MOVE_TOLERANCE_PX) this.clearLongPress(); // المستخدم بيعمل scroll مش long-press
  }

  clearLongPress(): void {
    if (this.longPressTimer) clearTimeout(this.longPressTimer);
    this.longPressTimer = null;
    this.pressStart = null;
  }

  /** بعد الضغطة الطويلة المتصفح بيعمل click عند رفع الصباع (ممكن يفتح الصورة/الملف) */
  onBubbleClick(event: MouseEvent): void {
    if (!this.longPressFired) return;
    event.preventDefault();
    event.stopPropagation();
    this.longPressFired = false;
  }

  /** يمنع قايمة المتصفح (Save image / Copy) على اللمس بس */
  onContextMenu(event: Event): void {
    if (window.matchMedia('(pointer: coarse)').matches) event.preventDefault();
  }




  
}