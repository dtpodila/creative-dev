"use client";

import { useState } from "react";
import { ChevronDown, ChevronUp, Pencil, Send } from "lucide-react";
import { Card } from "@/components/ui/Card";
import { formatCurrency } from "@/lib/utils";
import type { LineDetail } from "@/types/bill.types";
import { cn } from "@/lib/utils";

interface Props {
  line: LineDetail;
  onAssign: (lineId: number) => void;
  onSend: (lineId: number) => void;
}

export default function LineCostCard({ line, onAssign, onSend }: Props) {
  const [expanded, setExpanded] = useState(false);

  return (
    <Card className="gap-0 p-0 overflow-hidden">
      {/* Header */}
      <div className="p-4">
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1 min-w-0">
            <p className="font-bold text-gray-900 text-base">
              {line.assignedName || <span className="text-gray-400 italic">Unassigned</span>}
            </p>
            <p className="text-sm text-gray-500">{line.phoneNumber}</p>
          </div>
          <div className="text-right shrink-0">
            <p className="text-lg font-bold text-blue-600">{formatCurrency(line.totalLineCost)}</p>
            <p className="text-xs text-gray-400">total</p>
          </div>
        </div>

        {/* Cost breakdown row */}
        <div className="mt-3 grid grid-cols-3 gap-2 text-center">
          {[
            { label: "Plan", value: line.planCostShare },
            { label: "Equipment", value: line.equipmentCost },
            { label: "Services", value: line.servicesCost },
          ].map(({ label, value }) => (
            <div key={label} className="bg-gray-50 rounded-xl py-2">
              <p className="text-xs text-gray-500">{label}</p>
              <p className="text-sm font-semibold text-gray-800">{formatCurrency(value)}</p>
            </div>
          ))}
        </div>

        {/* Action buttons */}
        <div className="mt-3 flex gap-2">
          <button
            onClick={() => onAssign(line.lineId)}
            className="flex-1 flex items-center justify-center gap-1.5 h-9 rounded-xl bg-gray-100 text-sm font-medium text-gray-700 hover:bg-gray-200 transition-colors"
          >
            <Pencil className="w-4 h-4" />
            Assign Name
          </button>
          <button
            onClick={() => onSend(line.lineId)}
            disabled={!line.assignedContact}
            className="flex-1 flex items-center justify-center gap-1.5 h-9 rounded-xl bg-blue-600 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-40 transition-colors"
          >
            <Send className="w-4 h-4" />
            Send Bill
          </button>
        </div>
      </div>

      {/* Expandable charges detail */}
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full flex items-center justify-center gap-1 py-2 text-xs text-gray-400 border-t border-gray-100 hover:bg-gray-50 transition-colors"
      >
        {expanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
        {expanded ? "Hide details" : "View charge details"}
      </button>

      {expanded && (
        <div className="px-4 pb-4 flex flex-col gap-3 border-t border-gray-100 pt-3">
          {line.equipmentCharges.length > 0 && (
            <div>
              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Equipment</p>
              {line.equipmentCharges.map((e) => (
                <div key={e.equipmentChargeId} className="flex justify-between text-sm py-0.5">
                  <span className="text-gray-600 truncate pr-2">{e.deviceName || e.chargeName}</span>
                  <span className="font-medium text-gray-900 shrink-0">{formatCurrency(e.chargeAmount)}</span>
                </div>
              ))}
            </div>
          )}
          {line.serviceCharges.length > 0 && (
            <div>
              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Services</p>
              {line.serviceCharges.map((s) => (
                <div key={s.serviceChargeId} className="flex justify-between text-sm py-0.5">
                  <span className="text-gray-600 truncate pr-2">{s.chargeName}</span>
                  <span className="font-medium text-gray-900 shrink-0">{formatCurrency(s.chargeAmount)}</span>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </Card>
  );
}
