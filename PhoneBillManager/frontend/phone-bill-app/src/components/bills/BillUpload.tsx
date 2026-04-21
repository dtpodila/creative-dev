"use client";

import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";
import { Upload, FileText, X } from "lucide-react";
import { useRouter } from "next/navigation";
import { billService } from "@/services/billService";
import Button from "@/components/ui/Button";
import { cn } from "@/lib/utils";

export default function BillUpload() {
  const router = useRouter();
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState("");

  const onDrop = useCallback((accepted: File[]) => {
    if (accepted[0]) {
      setFile(accepted[0]);
      setError("");
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { "application/pdf": [".pdf"] },
    maxFiles: 1,
    maxSize: 20 * 1024 * 1024,
    onDropRejected: () => setError("Only PDF files up to 20 MB are accepted."),
  });

  const handleUpload = async () => {
    if (!file) return;
    setUploading(true);
    setError("");
    try {
      const result = await billService.uploadBill(file);
      router.push(`/bills/${result.billId}`);
    } catch {
      setError("Upload failed. Please try again.");
      setUploading(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <div
        {...getRootProps()}
        className={cn(
          "border-2 border-dashed rounded-2xl p-8 flex flex-col items-center gap-3 cursor-pointer transition-colors",
          isDragActive ? "border-blue-500 bg-blue-50" : "border-gray-300 bg-gray-50 hover:bg-gray-100"
        )}
      >
        <input {...getInputProps()} />
        <Upload className={cn("w-10 h-10", isDragActive ? "text-blue-500" : "text-gray-400")} />
        <div className="text-center">
          <p className="font-medium text-gray-700">
            {isDragActive ? "Drop your PDF here" : "Tap to select your bill PDF"}
          </p>
          <p className="text-sm text-gray-400 mt-1">PDF format · Max 20 MB</p>
        </div>
      </div>

      {file && (
        <div className="flex items-center gap-3 bg-blue-50 rounded-xl p-3">
          <FileText className="w-6 h-6 text-blue-600 shrink-0" />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-gray-900 truncate">{file.name}</p>
            <p className="text-xs text-gray-500">{(file.size / 1024 / 1024).toFixed(2)} MB</p>
          </div>
          <button onClick={() => setFile(null)} className="text-gray-400 hover:text-red-500 shrink-0">
            <X className="w-5 h-5" />
          </button>
        </div>
      )}

      {error && <p className="text-sm text-red-500 text-center">{error}</p>}

      <Button size="lg" onClick={handleUpload} disabled={!file} loading={uploading}>
        {uploading ? "Parsing Bill..." : "Upload & Parse Bill"}
      </Button>
    </div>
  );
}
