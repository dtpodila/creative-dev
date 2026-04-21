export interface RegisterRequest {
  fullName: string;
  email: string;
  mobileNumber: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  expiresAt: string;
}

export interface AuthUser {
  userId: number;
  fullName: string;
  email: string;
  expiresAt: string;
}
