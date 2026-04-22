"use client";

import { useState, useEffect } from "react";
import { X, MessageCircle, Phone } from "lucide-react";
import { billService } from "@/services/billService";
import Button from "@/components/ui/Button";
import type { BillDetail, LineDetail } from "@/types/bill.types";

interface Props {
  billId?: number;
  lineId?: number;
  onClose: () => void;
  onSent: (results: any[]) => void;
}

// Helper function to format currency
const formatCurrency = (amount: number) => `$${amount.toFixed(2)}`;

// Helper function to generate bill message
const generateMessage = (line: LineDetail, billInfo?: { period?: string; vendor?: string }) => {
  const vendor = billInfo?.vendor || "Phone Bill";
  const period = billInfo?.period || "";
  
  let message = `${vendor} Summary${period ? ` - ${period}` : ""}\n\n`;
  message += `📱 Line: ${line.phoneNumber}\n`;
  if (line.assignedName) {
    message += `👤 Name: ${line.assignedName}\n`;
  }
  message += `\n💰 Cost Breakdown:\n`;
  message += `• Plan Share: ${formatCurrency(line.planCostShare)}\n`;
  if (line.equipmentCost > 0) {
    message += `• Equipment: ${formatCurrency(line.equipmentCost)}\n`;
  }
  if (line.servicesCost > 0) {
    message += `• Services: ${formatCurrency(line.servicesCost)}\n`;
  }
  message += `\n✨ Total: ${formatCurrency(line.totalLineCost)}`;
  
  return message;
};

// Helper function to open SMS
const openSMS = (phoneNumber: string, message: string) => {
  // Format phone number (remove spaces, dashes, parentheses)
  const cleanNumber = phoneNumber.replace(/[\s\-()]/g, "");
  // Encode message for URL
  const encodedMessage = encodeURIComponent(message);
  
  // Use SMS protocol - works on both iOS and Android
  const smsUrl = `sms:${cleanNumber}${navigator.userAgent.match(/iPhone|iPad|iPod/i) ? '&' : '?'}body=${encodedMessage}`;
  window.open(smsUrl, '_blank');
};

// Helper function to open WhatsApp
const openWhatsApp = (phoneNumber: string, message: string) => {
  // Format phone number (remove spaces, dashes, parentheses, plus sign)
  const cleanNumber = phoneNumber.replace(/[\s\-()]/g, "");
  // Remove leading + if present, WhatsApp expects just the number with country code
  const whatsappNumber = cleanNumber.startsWith('+') ? cleanNumber.substring(1) : cleanNumber;
  // Encode message for URL
  const encodedMessage = encodeURIComponent(message);
  
  // Use WhatsApp web API
  const whatsappUrl = `https://wa.me/${whatsappNumber}?text=${encodedMessage}`;
  window.open(whatsappUrl, '_blank');
};

export default function SendBillModal({ billId, lineId, onClose, onSent }: Props) {
  const [channel, setChannel] = useState<"SMS" | "WhatsApp">("SMS");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [billData, setBillData] = useState<BillDetail | null>(null);
  const [selectedLines, setSelectedLines] = useState<number[]>([]);

  // Fetch bill data on mount
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        if (billId) {
          const bill = await billService.getBill(billId);
          setBillData(bill);
          
          if (lineId) {
            // If a specific line is requested, only select that line
            setSelectedLines([lineId]);
          } else {
            // Pre-select all assigned lines
            const assignedLineIds = bill.lines
              .filter(l => l.assignedContact)
              .map(l => l.lineId);
            setSelectedLines(assignedLineIds);
          }
        }
      } catch (err) {
        setError("Failed to load bill data.");
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, [billId, lineId]);

  const handleSend = () => {
    if (!billData) {
      setError("No data available to send.");
      return;
    }

    try {
      const results: any[] = [];
      
      // Send to selected lines from bill
      const linesToSend = billData.lines.filter(l => selectedLines.includes(l.lineId));
      
      if (linesToSend.length === 0) {
        setError("Please select at least one line to send to.");
        return;
      }

      // Filter out lines without assigned contact
      const validLines = linesToSend.filter(l => l.assignedContact);
      
      if (validLines.length === 0) {
        setError("Selected line(s) don't have contact information assigned.");
        return;
      }

      validLines.forEach(line => {
        const message = generateMessage(line, {
          period: billData.billingPeriod,
          vendor: billData.vendorName,
        });
        
        if (channel === "SMS") {
          openSMS(line.assignedContact!, message);
        } else {
          openWhatsApp(line.assignedContact!, message);
        }
        
        results.push({
          lineId: line.lineId,
          phoneNumber: line.phoneNumber,
          assignedName: line.assignedName,
          channel,
          status: "sent",
        });
      });
      
      onSent(results);
      onClose();
    } catch (err) {
      setError("Failed to open messaging app.");
    }
  };

  const toggleLineSelection = (lineId: number) => {
    setSelectedLines(prev =>
      prev.includes(lineId)
        ? prev.filter(id => id !== lineId)
        : [...prev, lineId]
    );
  };

  if (loading) {
    return (
      <div className="fixed inset-0 z-[60] flex items-end justify-center bg-black/40" onClick={onClose}>
        <div
          className="bg-white w-full max-w-md rounded-t-3xl p-6 flex items-center justify-center"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="text-gray-600">Loading...</div>
        </div>
      </div>
    );
  }

  // Filter lines based on whether we're sending to a specific line or all lines
  const assignedLines = billData?.lines.filter(l => l.assignedContact) || [];
  
  // If a specific lineId is provided, show only that line
  const displayLines = lineId 
    ? assignedLines.filter(l => l.lineId === lineId)
    : assignedLines;
    
  const hasMultipleLines = displayLines.length > 1;

  // Check if there are no lines with assigned contacts
  if (displayLines.length === 0) {
    return (
      <div className="fixed inset-0 z-[60] flex items-end justify-center bg-black/40" onClick={onClose}>
        <div
          className="bg-white w-full max-w-md rounded-t-3xl p-6 flex flex-col gap-4"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-gray-900">Send Bill Summary</h2>
            <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
              <X className="w-6 h-6" />
            </button>
          </div>
          <div className="text-center py-8">
            <p className="text-gray-600 mb-2">No lines with assigned contacts</p>
            <p className="text-sm text-gray-400">
              Please assign contact information to lines before sending bills.
            </p>
          </div>
          <Button size="lg" onClick={onClose}>
            Close
          </Button>
        </div>
      </div>
    );
  }

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
          {lineId 
            ? "Choose how to send this line's bill summary" 
            : "Select recipients and choose a messaging method"}
        </p>

        {/* Channel Selection */}
        <div>
          <h3 className="text-sm font-semibold text-gray-700 mb-2">Choose Method</h3>
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
        </div>

        {/* Line Selection (for multiple lines) */}
        {hasMultipleLines && (
          <div>
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-sm font-semibold text-gray-700">Select Recipients</h3>
              <button
                onClick={() => {
                  if (selectedLines.length === displayLines.length) {
                    setSelectedLines([]);
                  } else {
                    setSelectedLines(displayLines.map(l => l.lineId));
                  }
                }}
                className="text-xs text-blue-600 hover:text-blue-700"
              >
                {selectedLines.length === displayLines.length ? "Deselect All" : "Select All"}
              </button>
            </div>
            <div className="space-y-2 max-h-48 overflow-y-auto">
              {displayLines.map(line => (
                <label
                  key={line.lineId}
                  className="flex items-center gap-3 p-3 rounded-xl border border-gray-200 hover:bg-gray-50 cursor-pointer"
                >
                  <input
                    type="checkbox"
                    checked={selectedLines.includes(line.lineId)}
                    onChange={() => toggleLineSelection(line.lineId)}
                    className="w-4 h-4 text-blue-600 rounded"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-gray-900 truncate">
                      {line.assignedName || line.phoneNumber}
                    </div>
                    <div className="text-xs text-gray-500">
                      {line.phoneNumber} • {formatCurrency(line.totalLineCost)}
                    </div>
                  </div>
                </label>
              ))}
            </div>
          </div>
        )}

        {/* Single Line Info */}
        {!hasMultipleLines && displayLines.length === 1 && (
          <div className="p-3 rounded-xl bg-blue-50 border border-blue-200">
            <div className="text-sm font-medium text-gray-900">
              {displayLines[0].assignedName || displayLines[0].phoneNumber}
            </div>
            <div className="text-xs text-gray-600 mt-1">
              {displayLines[0].phoneNumber} • Total: {formatCurrency(displayLines[0].totalLineCost)}
            </div>
          </div>
        )}

        {error && <p className="text-sm text-red-500 text-center">{error}</p>}

        <div className="text-xs text-gray-500 bg-gray-50 p-3 rounded-lg">
          ℹ️ {channel === "SMS" 
            ? "Your default messaging app will open with the message pre-filled" 
            : "WhatsApp will open with the message ready to send"}
        </div>

        <Button 
          size="lg" 
          onClick={handleSend}
          disabled={loading || (hasMultipleLines && selectedLines.length === 0)}
        >
          Send via {channel}
        </Button>
      </div>
    </div>
  );
}
