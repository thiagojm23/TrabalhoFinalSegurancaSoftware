/**
 * Constantes da aplicação
 * Centralizadas para fácil manutenção
 */

// Rotas da aplicação
export const ROUTES = {
  HOME: "/home",
  LOGIN: "/login",
  ROOT: "/",
} as const;

// Chaves do localStorage
export const STORAGE_KEYS = {
  USER: "user",
  AUTH_TOKEN: "auth_token",
} as const;

// Mensagens de erro
export const ERROR_MESSAGES = {
  INVALID_CREDENTIALS: "Email ou senha inválidos",
  GENERIC_ERROR: "Erro ao processar requisição. Tente novamente.",
  NETWORK_ERROR: "Erro de conexão. Verifique sua internet.",
} as const;

// Configurações da aplicação
export const APP_CONFIG = {
  APP_NAME: "Dashboard App",
  VERSION: "1.0.0",
} as const;
