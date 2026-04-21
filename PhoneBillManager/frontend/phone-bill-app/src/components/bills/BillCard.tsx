import Link from "next/link";
import { FileText, ChevronRight } from "lucide-react";
import { Card } from "@/components/ui/Card";
import { formatCurrency, formatDate } from "@/lib/utils";
import type { BillSummary } from "@/types/bill.types";
import { cn } from "@/lib/utils";

const statusColors: Record<string, string> = {
  Parsed: "bg-green-100 text-green-700",
  Processing: "bg-yellow-100 text-yellow-700",
  Pending: "bg-gray-100 text-gray-600",
  Failed: "bg-red-100 text-red-600",
};

export default function BillCard({ bill }: { bill: BillSummary }) {
  return (
    <Link href={`/bills/${bill.billId}`}>
      <Card className="flex items-center gap-3 active:bg-gray-50 transition-colors">
        <div className="bg-blue-50 rounded-xl p-3 shrink-0">
          <FileText className="w-6 h-6 text-blue-600" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="font-semibold text-gray-900 truncate">{bill.vendorName || bill.billFileName}</p>
          <p className="text-sm text-gray-500">{bill.billingPeriod || formatDate(bill.billingDate)}</p>
          <div className="flex items-center gap-2 mt-1">
            <span className={cn("text-xs px-2 py-0.5 rounded-full font-medium", statusColors[bill.parseStatus] ?? "bg-gray-100 text-gray-600")}>
              {bill.parseStatus}
            </span>
            <span className="text-xs text-gray-400">{bill.numberOfLines} lines</span>
          </div>
        </div>
        <div className="text-right shrink-0">
          <p className="font-bold text-gray-900">{formatCurrency(bill.totalBillAmount)}</p>
          <ChevronRight className="w-4 h-4 text-gray-400 ml-auto mt-1" />
        </div>
      </Card>
    </Link>
  );
}
