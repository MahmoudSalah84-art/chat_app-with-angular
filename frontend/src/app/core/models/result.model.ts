import {User} from '../index';

export interface ApiErrorResponse {
  errors: string[];
}

export interface AuthResponse {
  token: string;
  user: User;
}


export interface AuthOperationResult {
  success: boolean;
  errorMessage?: string;
}