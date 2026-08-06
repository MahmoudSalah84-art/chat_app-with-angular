import { Component, input } from '@angular/core';

/**
 * Component بسيط لعرض صورة المستخدم مع دائرة حالة "متصل الآن" (اختياري).
 * قابل لإعادة الاستخدام في أي مكان: قائمة الشاتات، الهيدر، البروفايل...
 */
@Component({
  selector: 'app-avatar',
  imports: [],
  templateUrl: './avatar.html',
  styleUrl: './avatar.css',
})
export class Avatar {
  readonly src = input.required<string>();
  readonly alt = input<string>('avatar');
  readonly size = input<number>(48); // الحجم بالبكسل
  readonly showOnlineDot = input<boolean>(false);
  readonly isOnline = input<boolean>(false);
}
