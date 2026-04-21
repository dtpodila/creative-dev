"use client";

import { useEffect, useState } from "react";
import { CheckCircle, XCircle, Clock } from "lucide-react";
import TopBar from "@/components/layout/TopBar";
import { Card } from "@/components/ui/Card";
import { billService } from "@/services/billService";
import { notificationService } from "@/services/notificationService";
import type { BillSummary } from "@/types/bill.types";
import type { NotificationDto } from "@/types/notification.types";

const StatusIcon = ({ status }: { status: string }) => {
  if (status === "Sent" || status === "Delivered") return <CheckCircle className="w-5 h-5 text-green-500" />;
  if (status === "Failed") return <XCircle className="w-5 h-5 text-red-500" />;
  return <Clock className="w-5 h-5 text-yellow-500" />;
};

export default function NotificationsPage() {
  const [bills, setBills] = useState<BillSummary[]>([]);
  const [selectedBill, setSelectedBill] = useState<number | null>(null);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingNot, setLoadingNot] = useState(false);

  useEffect(() => {
    billService.getBills().then((b) => {
      setBills(b.filter((x) => x.parseStatus === "Parsed"));
      if (b.length > 0) setSelectedBill(b[0].billId);
    }).finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!selectedBill) return;
    setLoadingNot(true);
    notificationService.getNotifications(selectedBill).then(setNotifications).finally(() => setLoadingNot(false));
  }, [selectedBill]);

  return (
    <>
      <TopBar title="Notification History" />
      <div className="px-4 py-4 flex flex-col gap-4">
        {loading ? (
          <div className="flex justify-center py-16">
            <div className="animate-spin w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
          </div>
        ) : (
          <>
            {bills.length > 1 && (
              <select
                value={selectedBill ?? ""}
                onChange={(e) => setSelectedBill(parseInt(e.target.value))}
                className="w-full h-12 border border-gray-200 rounded-xl px-4 text-sm bg-white"
              >
                {bills.map((b) => (
                  <option key={b.billId} value={b.billId}>
                    {b.vendorName || b.billFileName} — {b.billingPeriod || b.createdAt.slice(0, 10)}
                  </option>
                ))}
              </select>
            )}

            {loadingNot ? (
              <div className="flex justify-center py-10">
                <div className="animate-spin w-6 h-6 border-4 border-blue-600 border-t-transparent rounded-full" />
              </div>
            ) : notifications.length === 0 ? (
              <div className="text-center py-16 text-gray-400">
                <p>No notifications sent for this bill yet.</p>
              </div>
            ) : (
              notifications.map((n) => (
                <Card key={n.notificationId} className="flex items-start gap-3">
                  <StatusIcon status={n.status} />
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-gray-900">{n.recipientName || n.recipientContact}</p>
                    <p className="text-sm text-gray-500">{n.recipientContact} · {n.channel}</p>
                    {n.errorMessage && <p className="text-xs text-red-500 mt-1">{n.errorMessage}</p>}
                    {n.sentAt && <p className="text-xs text-gray-400 mt-1">{new Date(n.sentAt).toLocaleString()}</p>}
                  </div>
                  <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${
                    n.status === "Sent" || n.status === "Delivered" ? "bg-green-100 text-green-700" :
                    n.status === "Failed" ? "bg-red-100 text-red-600" : "bg-yellow-100 text-yellow-600"
                  }`}>
                    {n.status}
                  </span>
                </Card>
              ))
            )}
          </>
        )}
      </div>
    </>
  );
}
