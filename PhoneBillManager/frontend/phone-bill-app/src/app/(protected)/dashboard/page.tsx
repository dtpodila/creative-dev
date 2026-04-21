"use client";

import { useEffect, useState } from "react";
import { Plus } from "lucide-react";
import Link from "next/link";
import TopBar from "@/components/layout/TopBar";
import BillCard from "@/components/bills/BillCard";
import { billService } from "@/services/billService";
import type { BillSummary } from "@/types/bill.types";

export default function DashboardPage() {
  const [bills, setBills] = useState<BillSummary[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    billService.getBills().then(setBills).finally(() => setLoading(false));
  }, []);

  return (
    <>
      <TopBar title="My Bills" />
      <div className="px-4 py-4 flex flex-col gap-3">
        {loading ? (
          <div className="flex justify-center py-16">
            <div className="animate-spin w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
          </div>
        ) : bills.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-20 gap-4 text-center">
            <div className="bg-blue-50 rounded-full p-6">
              <Plus className="w-10 h-10 text-blue-600" />
            </div>
            <div>
              <p className="font-semibold text-gray-800 text-lg">No bills yet</p>
              <p className="text-sm text-gray-500 mt-1">Upload your first phone bill to get started</p>
            </div>
            <Link
              href="/bills/upload"
              className="mt-2 bg-blue-600 text-white font-semibold px-6 py-3 rounded-xl hover:bg-blue-700 transition-colors"
            >
              Upload Bill
            </Link>
          </div>
        ) : (
          bills.map((bill) => <BillCard key={bill.billId} bill={bill} />)
        )}
      </div>
    </>
  );
}
