import api from "./api";
import type { LineDetail } from "@/types/bill.types";

export interface AssignLineRequest {
  assignedName: string;
  assignedContact: string;
}

export const lineService = {
  async getLines(billId: number): Promise<LineDetail[]> {
    const res = await api.get<LineDetail[]>(`/bills/${billId}/lines`);
    return res.data;
  },

  async assignLine(lineId: number, data: AssignLineRequest): Promise<LineDetail> {
    const res = await api.put<LineDetail>(`/lines/${lineId}/assign`, data);
    return res.data;
  },
};
