import api from "./api";
import type { BillDetail, BillSummary } from "@/types/bill.types";

export const billService = {
  async getBills(): Promise<BillSummary[]> {
    const res = await api.get<BillSummary[]>("/bills");
    return res.data;
  },

  async getBill(id: number): Promise<BillDetail> {
    const res = await api.get<BillDetail>(`/bills/${id}`);
    return res.data;
  },

  async uploadBill(file: File): Promise<BillSummary> {
    const form = new FormData();
    form.append("file", file);
    const res = await api.post<BillSummary>("/bills/upload", form, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    return res.data;
  },

  async deleteBill(id: number): Promise<void> {
    await api.delete(`/bills/${id}`);
  },
};
