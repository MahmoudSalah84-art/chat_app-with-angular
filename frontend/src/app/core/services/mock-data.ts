import { Chat } from '../models/chat.model';
import { Message } from '../models/message.model';
import { User } from '../models/user.model';
import { MessageStatus, MessageType } from '../enums/message-status.enum';

/**
 * المستخدم الحالي (اللي داخل بحسابه دلوقتي).
 */
export const CURRENT_USER: User = {
  id: 'user-0',
  name: 'أحمد محمود',
  avatarUrl: 'https://i.pravatar.cc/150?img=12',
  isOnline: true,
  about: 'متاح',
};

/**
 * باقي المستخدمين الوهميين اللي هنتكلم معاهم.
 */
export const MOCK_USERS: User[] = [
  {
    id: 'user-1',
    name: 'سارة عبد الله',
    avatarUrl: 'https://i.pravatar.cc/150?img=5',
    isOnline: true,
    about: 'بحب القهوة ☕',
  },
  {
    id: 'user-2',
    name: 'محمد علي',
    avatarUrl: 'https://i.pravatar.cc/150?img=33',
    isOnline: false,
    lastSeen: new Date(Date.now() - 1000 * 60 * 45),
    about: 'مشغول شوية',
  },
  {
    id: 'user-3',
    name: 'ياسمين خالد',
    avatarUrl: 'https://i.pravatar.cc/150?img=9',
    isOnline: true,
    about: 'Live, Laugh, Code',
  },
  {
    id: 'user-4',
    name: 'عمر حسن',
    avatarUrl: 'https://i.pravatar.cc/150?img=51',
    isOnline: false,
    lastSeen: new Date(Date.now() - 1000 * 60 * 60 * 5),
  },
  {
    id: 'user-5',
    name: 'فريق التطوير',
    avatarUrl: 'https://i.pravatar.cc/150?img=60',
    isOnline: true,
  },
];

let messageIdCounter = 1;
function createMessage(
  chatId: string,
  senderId: string,
  content: string,
  minutesAgo: number,
  status: MessageStatus = MessageStatus.Read,
): Message {
  return {
    id: `msg-${messageIdCounter++}`,
    chatId,
    senderId,
    type: MessageType.Text,
    content,
    timestamp: new Date(Date.now() - minutesAgo * 60 * 1000),
    status,
  };
}

// رسائل محادثة سارة
const chat1Messages: Message[] = [
  createMessage('chat-1', 'user-1', 'أهلاً! عامل إيه؟', 120),
  createMessage('chat-1', 'user-0', 'الحمد لله تمام، وإنتِ عاملة إيه؟', 118),
  createMessage('chat-1', 'user-1', 'كويسة الحمد لله 🙌', 115),
  createMessage('chat-1', 'user-1', 'شفت المشروع الجديد اللي بتشتغل عليه؟', 60),
  createMessage('chat-1', 'user-0', 'أيوة، شغال عليه دلوقتي فعلاً 😄', 58, MessageStatus.Delivered),
];

// رسائل محادثة محمد
const chat2Messages: Message[] = [
  createMessage('chat-2', 'user-2', 'تمام يا صاحبي هرد عليك بعدين', 300),
  createMessage('chat-2', 'user-0', 'تمام خد وقتك', 295, MessageStatus.Read),
];

// رسائل محادثة ياسمين
const chat3Messages: Message[] = [
  createMessage('chat-3', 'user-3', 'الكود اشتغل تمام النهاردة 🔥', 20),
  createMessage('chat-3', 'user-0', 'يا سلام! إزاي؟', 18),
  createMessage('chat-3', 'user-3', 'كنت مستخدم RxJS غلط، صلحته', 15),
  createMessage('chat-3', 'user-3', 'تحب أبعتلك الـ commit؟', 2, MessageStatus.Sent),
];

// رسائل جروب فريق التطوير
const chat5Messages: Message[] = [
  createMessage('chat-5', 'user-4', 'الاجتماع الساعة كام النهاردة؟', 500),
  createMessage('chat-5', 'user-5', 'الساعة 4 العصر ⏰', 480),
  createMessage('chat-5', 'user-0', 'تمام هكون جاهز', 475, MessageStatus.Read),
];

export const MOCK_MESSAGES: Message[] = [
  ...chat1Messages,
  ...chat2Messages,
  ...chat3Messages,
  ...chat5Messages,
];

export const MOCK_CHATS: Chat[] = [
  {
    id: 'chat-1',
    isGroup: false,
    name: 'سارة عبد الله',
    avatarUrl: MOCK_USERS[0].avatarUrl,
    participants: [CURRENT_USER, MOCK_USERS[0]],
    lastMessage: chat1Messages[chat1Messages.length - 1],
    unreadCount: 0,
  },
  {
    id: 'chat-2',
    isGroup: false,
    name: 'محمد علي',
    avatarUrl: MOCK_USERS[1].avatarUrl,
    participants: [CURRENT_USER, MOCK_USERS[1]],
    lastMessage: chat2Messages[chat2Messages.length - 1],
    unreadCount: 0,
  },
  {
    id: 'chat-3',
    isGroup: false,
    name: 'ياسمين خالد',
    avatarUrl: MOCK_USERS[2].avatarUrl,
    participants: [CURRENT_USER, MOCK_USERS[2]],
    lastMessage: chat3Messages[chat3Messages.length - 1],
    unreadCount: 2,
  },
  {
    id: 'chat-4',
    isGroup: false,
    name: 'عمر حسن',
    avatarUrl: MOCK_USERS[3].avatarUrl,
    participants: [CURRENT_USER, MOCK_USERS[3]],
    unreadCount: 0,
  },
  {
    id: 'chat-5',
    isGroup: true,
    name: 'فريق التطوير 🚀',
    avatarUrl: 'https://i.pravatar.cc/150?img=60',
    participants: [CURRENT_USER, MOCK_USERS[3], MOCK_USERS[4]],
    lastMessage: chat5Messages[chat5Messages.length - 1],
    unreadCount: 1,
  },
];
