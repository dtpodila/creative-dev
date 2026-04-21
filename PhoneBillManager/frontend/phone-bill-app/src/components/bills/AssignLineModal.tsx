"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { X } from "lucide-react";
import { lineService } from "@/services/lineService";
import Input from "@/components/ui/Input";
import Button from "@/components/ui/Button";
import type { LineDetail } from "@/types/bill.types";

const schema = z.object({
  assignedName: z.string().min(1, "Name is required"),
  assignedContact: z.string().min(10, "Enter a valid phone number (E.164 e.g. +15551234567)"),
});
type FormData = z.infer<typeof schema>;

interface Props {
  line: LineDetail;
  onClose: () => void;
  onSaved: (updated: LineDetail) => void;
}

export default function AssignLineModal({ line, onClose, onSaved }: Props) {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      assignedName: line.assignedName ?? "",
      assignedContact: line.assignedContact ?? "",
    },
  });

  const onSubmit = async (data: FormData) => {
    const updated = await lineService.assignLine(line.lineId, data);
    onSaved(updated);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-[60] flex items-end justify-center bg-black/40" onClick={onClose}>
      <div
        className="bg-white w-full max-w-md max-h-[calc(100vh-100px)] rounded-t-3xl p-6 flex flex-col gap-4 overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-bold text-gray-900">Assign Line</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-6 h-6" />
          </button>
        </div>
        <p className="text-sm text-gray-500">Line: <span className="font-medium text-gray-700">{line.phoneNumber}</span></p>

        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <Input
            id="assignedName"
            label="Person's Name"
            placeholder="e.g. John Doe"
            error={errors.assignedName?.message}
            {...register("assignedName")}
          />
          <Input
            id="assignedContact"
            label="WhatsApp / SMS Number"
            type="tel"
            placeholder="+15551234567"
            error={errors.assignedContact?.message}
            {...register("assignedContact")}
          />
          <Button type="submit" size="lg" loading={isSubmitting}>Save</Button>
        </form>
      </div>
    </div>
  );
}
