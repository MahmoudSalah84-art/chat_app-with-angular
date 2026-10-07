import { Chat } from '../models/chat.model';
import { Message } from '../models/message.model';
import { MessageStatus } from '../enums/message-status.enum';
import { toUtcMs } from './date';

export function getMessageStatus(msg: Message, chat: Chat | undefined, myId: string): MessageStatus {
  const others = (chat?.participants ?? []).filter((p) => p.id !== myId);
  if (!others.length) return MessageStatus.Sent; // this chat for me only

  const sentMs = toUtcMs(msg.sentAt);
  const receipts = chat?.receipts ?? [];

  const check = (pick: 'lastDeliveredAt' | 'lastReadAt') =>
    others.every((p) => toUtcMs(receipts.find((r) => r.userId === p.id)?.[pick]) >= sentMs);

  if (check('lastReadAt')) return MessageStatus.Read;
  if (check('lastDeliveredAt')) return MessageStatus.Delivered;
  return MessageStatus.Sent;
}