"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Home, Upload, Bell, LogOut } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { cn } from "@/lib/utils";

const navItems = [
  { href: "/dashboard", label: "Home", icon: Home },
  { href: "/bills/upload", label: "Upload", icon: Upload },
  { href: "/notifications", label: "Alerts", icon: Bell },
];

export default function BottomNav() {
  const pathname = usePathname();
  const { logout } = useAuth();

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 bg-white border-t border-gray-200 flex items-center justify-around h-16 px-2 max-w-md mx-auto">
      {navItems.map(({ href, label, icon: Icon }) => {
        const active = pathname === href || pathname.startsWith(href + "/");
        return (
          <Link
            key={href}
            href={href}
            className={cn(
              "flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl transition-colors",
              active ? "text-blue-600" : "text-gray-500 hover:text-gray-700"
            )}
          >
            <Icon className="w-6 h-6" strokeWidth={active ? 2.5 : 1.8} />
            <span className="text-xs font-medium">{label}</span>
          </Link>
        );
      })}
      <button
        onClick={logout}
        className="flex flex-col items-center gap-0.5 px-3 py-1 rounded-xl text-gray-500 hover:text-red-500 transition-colors"
      >
        <LogOut className="w-6 h-6" strokeWidth={1.8} />
        <span className="text-xs font-medium">Logout</span>
      </button>
    </nav>
  );
}
