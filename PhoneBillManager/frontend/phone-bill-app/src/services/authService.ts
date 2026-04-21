import api from "./api";
import type { AuthResponse, LoginRequest, RegisterRequest } from "@/types/auth.types";

export const authService = {
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>("/auth/register", data);
    return res.data;
  },

  async login(data: LoginRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>("/auth/login", data);
    return res.data;
  },
};
