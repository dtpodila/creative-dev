"use client";

import { useState } from "react";
import { X, MessageCircle, Phone } from "lucide-react";
import { notificationService } from "@/services/notificationService";
import Button from "@/components/ui/Button";
import type { NotificationDto } from "@/types/notification.types";

interface Props {
  billId?: number;
  lineId?: number;
  onClose: () => void;
  onSent: (results: NotificationDto[]) => void;
}

export default function SendBillModal({ billId, lineId, onClose, onSent }: Props) {
  const [channel, setChannel] = useState<"SMS" | "WhatsApp">("SMS");
  const [sending, setSending] = useState(false);
  const [error, setError] = useState("");

  const handleSend = async () => {
    setSending(true);
    setError("");
    try {
      let results: NotificationDto[];
      if (billId) {
        results = await notificationService.sendBill(billId, { channel });
      } else if (lineId) {
        const r = await notificationService.sendLine(lineId, { channel });
        results = [r];
      } else {
        results = [];
      }
      onSent(results);
      onClose();
    } catch {
      setError("Failed to send. Please check Twilio settings.");
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="fixed inset-0 z-[60] flex items-end justify-center bg-black/40" onClick={onClose}>
      <div
        className="bg-white w-full max-w-md max-h-[calc(100vh-100px)] rounded-t-3xl p-6 flex flex-col gap-4 overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-bold text-gray-900">Send Bill Summary</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X className="w-6 h-6" />
          </button>
        </div>

        <p className="text-sm text-gray-500">
          {billId ? "Send to all assigned lines" : "Send to this line"}
        </p>

        <div className="grid grid-cols-2 gap-3">
          {(["SMS", "WhatsApp"] as const).map((ch) => (
            <button
              key={ch}
              onClick={() => setChannel(ch)}
              className={`flex flex-col items-center gap-2 p-4 rounded-2xl border-2 transition-colors ${
                channel === ch
                  ? "border-blue-600 bg-blue-50 text-blue-700"
                  : "border-gray-200 text-gray-600 hover:bg-gray-50"
              }`}
            >
              {ch === "WhatsApp" ? <MessageCircle className="w-7 h-7" /> : <Phone className="w-7 h-7" />}
              <span className="text-sm font-semibold">{ch}</span>
            </button>
          ))}
        </div>

        {error && <p className="text-sm text-red-500 text-center">{error}</p>}

        <Button size="lg" onClick={handleSend} loading={sending}>
          Send via {channel}
        </Button>
      </div>
    </div>
  );
}
