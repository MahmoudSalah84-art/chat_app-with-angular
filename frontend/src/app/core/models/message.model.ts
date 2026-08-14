import { MessageType } from '../enums/message-type.enum';

/** نفس شكل MessageDto في الـ Backend بالظبط */
export interface Message {
  id: string;
  chatId: string;
  senderId: string;
  type: MessageType;
  content: string;
  sentAt: string;
  replyToMessageId?: string | null;
  isEdited: boolean;
  isDeleted: boolean;
}
