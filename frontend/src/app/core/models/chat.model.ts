import {User, Message} from '../index';

/**
 * يمثل محادثة (فردية أو جماعية).
 */
export interface Chat {
  id: string;
  isGroup: boolean;
  name: string; // اسم الشخص أو اسم الجروب
  avatarUrl: string;
  participants: User[]; // الأعضاء في المحادثة
  lastMessage?: Message; // آخر رسالة لعرضها في القائمة
  unreadCount: number; // عدد الرسائل الغير مقروءة
  isPinned?: boolean;
  isMuted?: boolean;
}
