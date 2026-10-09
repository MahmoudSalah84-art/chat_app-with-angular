import {MessageType} from '../index';

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
  attachment?: Attachment | null;
  reactions: Reaction[];
}

export interface Attachment {
  url: string;
  fileName: string;
  contentType: string;
  sizeInBytes: number;
}
export interface Reaction {
  userId: string;
  emoji: string;
}
 