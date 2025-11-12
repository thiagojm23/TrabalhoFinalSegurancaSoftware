/**
 * Tipos compartilhados da aplicação
 * Seguindo convenções TypeScript
 */

export interface User {
  email: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  token: string;
}

export interface ApiError {
  message: string;
  code?: string;
  details?: unknown;
}
