"use client";

import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { jwtDecode } from "jwt-decode";
import type { AuthUser } from "@/types/auth.types";

interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  exp: number;
}

interface AuthContextType {
  user: AuthUser | null;
  token: string | null;
  login: (token: string, fullName: string, email: string, expiresAt: string) => void;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem("token");
    if (stored) {
      try {
        const decoded = jwtDecode<JwtPayload>(stored);
        if (decoded.exp * 1000 > Date.now()) {
          setToken(stored);
          setUser({
            userId: parseInt(decoded.sub),
            fullName: decoded.name,
            email: decoded.email,
            expiresAt: new Date(decoded.exp * 1000).toISOString(),
          });
        } else {
          localStorage.removeItem("token");
        }
      } catch {
        localStorage.removeItem("token");
      }
    }
  }, []);

  const login = (token: string, fullName: string, email: string, expiresAt: string) => {
    localStorage.setItem("token", token);
    setToken(token);
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      setUser({ userId: parseInt(decoded.sub), fullName, email, expiresAt });
    } catch {
      setUser(null);
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout, isAuthenticated: !!user }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}
