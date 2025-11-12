import { createContext, useContext, useState } from "react";
import type { ReactNode } from "react";
import type { User } from "../types";
import axios, { isAxiosError } from "../lib/axios";

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  const login = async (email: string, password: string): Promise<boolean> => {
    try {
      const response = await axios.post("/api/TrabalhoSF/Usuario/Logar", {
        email,
        senha: password,
      });

      console.log("Login realizado:", response.data);

      // Backend já salva cookie httpOnly, apenas setamos o user
      const userData: User = {
        email: email,
      };
      setUser(userData);
      return true;
    } catch (error) {
      console.error("Erro ao fazer login:", error);
      if (isAxiosError(error) && error.response?.status === 401) {
        return false;
      }
      throw error;
    }
  };

  const logout = () => {
    setUser(null);
    // Opcionalmente, chamar endpoint de logout do backend
  };

  const value = {
    user,
    login,
    logout,
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("Context indefinido");
  }
  return context;
}
