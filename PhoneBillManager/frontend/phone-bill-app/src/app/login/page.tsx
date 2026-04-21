import LoginForm from "@/components/auth/LoginForm";
import { Smartphone } from "lucide-react";

export const metadata = { title: "Sign In — Phone Bill Manager" };

export default function LoginPage() {
  return (
    <div className="min-h-screen flex flex-col justify-center px-6 py-12 bg-white">
      <div className="mb-10 flex flex-col items-center gap-3">
        <div className="bg-blue-600 rounded-2xl p-4">
          <Smartphone className="w-9 h-9 text-white" />
        </div>
        <h1 className="text-2xl font-bold text-gray-900">Phone Bill Manager</h1>
        <p className="text-gray-500 text-sm text-center">Sign in to manage and split your phone bills</p>
      </div>
      <LoginForm />
    </div>
  );
}
