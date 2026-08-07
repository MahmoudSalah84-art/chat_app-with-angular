/**
 * يمثل بيانات المستخدم الأساسية داخل التطبيق.
 */
export interface User {
  id: string;
  name: string;
  avatarUrl: string;
  about?: string;
  phone?: string;
  email?: string;
  isOnline: boolean;
  lastSeen?: Date;
}
