import { User } from './user.model';

export interface ApiErrorResponse {
  errors: string[];
}

export interface AuthResponse {
  token: string;
  user: User;
}
