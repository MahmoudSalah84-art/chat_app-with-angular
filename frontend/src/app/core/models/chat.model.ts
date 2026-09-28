import {User, Message} from '../index';

export interface Chat {
  id: string;
  isGroup: boolean;
  name: string; 
  avatarUrl: string;
  participants: User[];
  lastMessage?: Message; 
  unreadCount: number; 
  isPinned?: boolean;
  isMuted?: boolean;
}
