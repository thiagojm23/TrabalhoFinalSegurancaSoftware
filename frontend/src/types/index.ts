/**
 * Tipos compartilhados da aplicação
 * Seguindo convenções TypeScript
 */

export interface User {
  id: string
  name: string
  email: string
  avatar?: string
  role?: UserRole
}

export type UserRole = 'admin' | 'user' | 'guest'

export interface LoginCredentials {
  email: string
  password: string
}

export interface AuthResponse {
  user: User
  token: string
}

export interface ApiError {
  message: string
  code?: string
  details?: unknown
}
