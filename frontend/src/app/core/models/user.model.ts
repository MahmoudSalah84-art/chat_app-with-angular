/**
 * يمثل بيانات المستخدم الأساسية داخل التطبيق.
 */
export interface User {
  id: string;
  name: string;
  email: string;
  avatarUrl: string;
  about?: string | null;
  phoneNumber?: string | null;
  isOnline: boolean;
  lastSeenAt?: string | null; // ISO string جاي من الـ API، بنحوّله لـ Date وقت العرض
}
