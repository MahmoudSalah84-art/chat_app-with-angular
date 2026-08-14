/**
 * حالة تسليم الرسالة (تظهر بعلامات الصح زي واتساب).
 */
export enum MessageStatus {
  Sending = 'sending', // بتتبعت لسه (ساعة صغيرة)
  Sent = 'sent', // اتبعتت (✓)
  Delivered = 'delivered', // وصلت للطرف التاني (✓✓ رمادي)
  Read = 'read', // اتقرت (✓✓ أزرق)
  Failed = 'failed', // فشل الإرسال
}
