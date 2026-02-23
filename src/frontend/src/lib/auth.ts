import NextAuth from "next-auth";
import type { NextAuthConfig } from "next-auth";
import type { UserRole, AuthResponse, UserResponse } from "@/api/generated/api-schema";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

const authConfig: NextAuthConfig = {
  pages: {
    signIn: "/login",
    error: "/login",
  },
  callbacks: {
    authorized({ auth, request: { nextUrl } }) {
      const isLoggedIn = !!auth?.user;
      const isOnDashboard = nextUrl.pathname.startsWith("/dashboard");
      const isOnLoginPage = nextUrl.pathname === "/login";

      if (isOnDashboard) {
        if (isLoggedIn) return true;
        return false;
      } else if (isLoggedIn && isOnLoginPage) {
        return Response.redirect(new URL("/dashboard", nextUrl));
      }
      return true;
    },
  },
  providers: [],
};

export const { handlers, signIn, signOut, auth } = NextAuth(authConfig);

// Custom auth functions for API interaction
export async function login(email: string, password: string): Promise<AuthResponse> {
  const response = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || "Login failed");
  }

  return response.json();
}

export async function register(email: string, password: string, fullName: string): Promise<AuthResponse> {
  const response = await fetch(`${API_URL}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password, fullName }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || "Registration failed");
  }

  return response.json();
}

export async function getCurrentUser(token: string): Promise<UserResponse> {
  const response = await fetch(`${API_URL}/api/auth/me`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to get current user");
  }

  return response.json();
}
