"use client";

import { useAuth } from "@/context/AuthContext";
import { ChevronLeft } from "lucide-react";
import { useRouter } from "next/navigation";

interface TopBarProps {
  title: string;
  showBack?: boolean;
}

export default function TopBar({ title, showBack }: TopBarProps) {
  const { user } = useAuth();
  const router = useRouter();

  return (
    <header className="sticky top-0 z-40 bg-white border-b border-gray-100 flex items-center h-14 px-4 gap-3">
      {showBack && (
        <button onClick={() => router.back()} className="text-gray-600 hover:text-gray-900 -ml-1">
          <ChevronLeft className="w-6 h-6" />
        </button>
      )}
      <h1 className="flex-1 text-lg font-bold text-gray-900 truncate">{title}</h1>
      {!showBack && user && (
        <span className="text-sm text-gray-500 truncate max-w-[120px]">{user.fullName}</span>
      )}
    </header>
  );
}
