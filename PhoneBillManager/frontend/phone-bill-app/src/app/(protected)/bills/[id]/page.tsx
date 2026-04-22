"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { Send } from "lucide-react";
import TopBar from "@/components/layout/TopBar";
import LineCostCard from "@/components/bills/LineCostCard";
import AssignLineModal from "@/components/bills/AssignLineModal";
import SendBillModal from "@/components/notifications/SendBillModal";
import { Card } from "@/components/ui/Card";
import { billService } from "@/services/billService";
import { formatCurrency } from "@/lib/utils";
import type { BillDetail, LineDetail } from "@/types/bill.types";
import type { NotificationDto } from "@/types/notification.types";

export default function BillDetailPage() {
  const { id } = useParams<{ id: string }>();
  const billId = parseInt(id);

  const [bill, setBill] = useState<BillDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [assigningLine, setAssigningLine] = useState<LineDetail | null>(null);
  const [sendingBill, setSendingBill] = useState(false);
  const [sendingLineId, setSendingLineId] = useState<number | null>(null);
  const [sentResults, setSentResults] = useState<NotificationDto[]>([]);

  useEffect(() => {
    billService.getBill(billId).then(setBill).finally(() => setLoading(false));
  }, [billId]);

  const handleLineSaved = (updated: LineDetail) => {
    if (!bill) return;
    setBill({ ...bill, lines: bill.lines.map((l) => (l.lineId === updated.lineId ? updated : l)) });
  };

  if (loading) {
    return (
      <>
        <TopBar title="Bill Details" showBack />
        <div className="flex justify-center py-20">
          <div className="animate-spin w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
        </div>
      </>
    );
  }

  if (!bill) {
    return (
      <>
        <TopBar title="Bill Details" showBack />
        <div className="px-4 py-10 text-center text-gray-500">Bill not found.</div>
      </>
    );
  }

  return (
    <>
      <TopBar title={bill.vendorName || "Bill Details"} showBack />
      <div className="px-4 py-4 flex flex-col gap-4">

        {/* Summary card */}
        <Card>
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-gray-500">{bill.billingPeriod || "Billing Period"}</p>
              {bill.accountNumber && <p className="text-xs text-gray-400 mt-0.5">Acct: {bill.accountNumber}</p>}
            </div>
            <div className="text-right">
              <p className="text-2xl font-bold text-gray-900">{formatCurrency(bill.totalBillAmount)}</p>
              <p className="text-xs text-gray-400">{bill.numberOfLines} lines</p>
            </div>
          </div>
          <div className="mt-4 grid grid-cols-3 gap-2 text-center">
            {[
              { label: "Plans", value: bill.totalPlanAmount },
              { label: "Equipment", value: bill.totalEquipmentAmount },
              { label: "Services", value: bill.totalServicesAmount },
            ].map(({ label, value }) => (
              <div key={label} className="bg-gray-50 rounded-xl py-2">
                <p className="text-xs text-gray-400">{label}</p>
                <p className="text-sm font-semibold text-gray-800">{formatCurrency(value)}</p>
              </div>
            ))}
          </div>
        </Card>

        {/* Plan share note */}
        {bill.numberOfLines > 0 && (
          <p className="text-xs text-gray-400 text-center">
            Plan cost shared equally — {formatCurrency(bill.totalPlanAmount / bill.numberOfLines)} per line
          </p>
        )}

        {/* Send all button */}
        <button
          onClick={() => setSendingBill(true)}
          className="flex items-center justify-center gap-2 w-full h-11 bg-green-600 text-white font-semibold rounded-xl hover:bg-green-700 transition-colors"
        >
          <Send className="w-5 h-5" />
          Send All Bills
        </button>

        {/* Lines */}
        <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mt-2">Lines on Account</h2>
        {bill.lines.map((line) => (
          <LineCostCard
            key={line.lineId}
            line={line}
            onAssign={(lineId) => setAssigningLine(bill.lines.find((l) => l.lineId === lineId) ?? null)}
            onSend={(lineId) => setSendingLineId(lineId)}
          />
        ))}

        {/* Plan charges detail */}
        {bill.planCharges.length > 0 && (
          <>
            <h2 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mt-2">Plan Charges</h2>
            <Card>
              {bill.planCharges.map((p) => (
                <div key={p.planChargeId} className="flex justify-between text-sm py-1 border-b border-gray-50 last:border-0">
                  <span className="text-gray-600">{p.chargeName}</span>
                  <span className="font-medium text-gray-900">{formatCurrency(p.chargeAmount)}</span>
                </div>
              ))}
            </Card>
          </>
        )}

        {/* Sent results */}
        {sentResults.length > 0 && (
          <Card>
            <p className="text-sm font-semibold text-gray-700 mb-2">Notification Results</p>
            {sentResults.map((r) => (
              <div key={r.notificationId} className="flex justify-between text-sm py-0.5">
                <span className="text-gray-600">{r.recipientName || r.recipientContact}</span>
                <span className={r.status === "Sent" ? "text-green-600 font-medium" : "text-red-500 font-medium"}>
                  {r.status}
                </span>
              </div>
            ))}
          </Card>
        )}
      </div>

      {/* Modals */}
      {assigningLine && (
        <AssignLineModal
          line={assigningLine}
          onClose={() => setAssigningLine(null)}
          onSaved={handleLineSaved}
        />
      )}
      {sendingBill && (
        <SendBillModal
          billId={billId}
          onClose={() => setSendingBill(false)}
          onSent={(r) => setSentResults(r)}
        />
      )}
      {sendingLineId !== null && (
        <SendBillModal
          billId={billId}
          lineId={sendingLineId}
          onClose={() => setSendingLineId(null)}
          onSent={(r) => setSentResults(r)}
        />
      )}
    </>
  );
}
