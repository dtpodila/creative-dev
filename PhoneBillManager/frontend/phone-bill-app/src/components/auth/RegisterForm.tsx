"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { authService } from "@/services/authService";
import Input from "@/components/ui/Input";
import Button from "@/components/ui/Button";

const schema = z.object({
  fullName: z.string().min(2, "Full name is required"),
  email: z.string().email("Invalid email"),
  mobileNumber: z.string().min(10, "Enter a valid mobile number"),
  password: z.string().min(8, "Password must be at least 8 characters"),
});
type FormData = z.infer<typeof schema>;

export default function RegisterForm() {
  const { login } = useAuth();
  const router = useRouter();
  const [serverError, setServerError] = useState("");

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: FormData) => {
    setServerError("");
    try {
      const res = await authService.register(data);
      login(res.token, res.fullName, res.email, res.expiresAt);
      router.replace("/dashboard");
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setServerError(msg || "Registration failed. Please try again.");
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      <Input
        id="fullName"
        label="Full Name"
        placeholder="John Doe"
        autoComplete="name"
        error={errors.fullName?.message}
        {...register("fullName")}
      />
      <Input
        id="email"
        label="Email"
        type="email"
        placeholder="you@example.com"
        autoComplete="email"
        error={errors.email?.message}
        {...register("email")}
      />
      <Input
        id="mobileNumber"
        label="Mobile Number"
        type="tel"
        placeholder="+1 (555) 000-0000"
        autoComplete="tel"
        error={errors.mobileNumber?.message}
        {...register("mobileNumber")}
      />
      <Input
        id="password"
        label="Password"
        type="password"
        placeholder="Min. 8 characters"
        autoComplete="new-password"
        error={errors.password?.message}
        {...register("password")}
      />

      {serverError && (
        <p className="text-sm text-red-500 text-center">{serverError}</p>
      )}

      <Button type="submit" size="lg" loading={isSubmitting} className="mt-2">
        Create Account
      </Button>

      <p className="text-center text-sm text-gray-500">
        Already have an account?{" "}
        <Link href="/login" className="text-blue-600 font-medium">
          Sign In
        </Link>
      </p>
    </form>
  );
}
