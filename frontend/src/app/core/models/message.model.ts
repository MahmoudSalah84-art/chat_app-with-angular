import { MessageStatus, MessageType } from '../enums/message-status.enum';

/**
 * يمثل رسالة واحدة داخل محادثة معينة.
 */
export interface Message {
  id: string;
  chatId: string;
  senderId: string;
  type: MessageType;
  content: string; // نص الرسالة، أو رابط الميديا في حالة صورة/فيديو
  timestamp: Date;
  status: MessageStatus;
  replyToMessageId?: string; // لو الرسالة دي رد على رسالة تانية
  isEdited?: boolean; // اتعدلت بعد ما اتبعتت
  isDeleted?: boolean; // اتحذفت (بيتم استبدال المحتوى بنص "تم الحذف")
}
