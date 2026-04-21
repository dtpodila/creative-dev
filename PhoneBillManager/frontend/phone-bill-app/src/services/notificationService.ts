import api from "./api";
import type { NotificationDto, SendNotificationRequest } from "@/types/notification.types";

export const notificationService = {
  async sendBill(billId: number, data: SendNotificationRequest): Promise<NotificationDto[]> {
    const res = await api.post<NotificationDto[]>(`/notifications/send/${billId}`, data);
    return res.data;
  },

  async sendLine(lineId: number, data: SendNotificationRequest): Promise<NotificationDto> {
    const res = await api.post<NotificationDto>(`/notifications/send-line/${lineId}`, data);
    return res.data;
  },

  async getNotifications(billId: number): Promise<NotificationDto[]> {
    const res = await api.get<NotificationDto[]>(`/notifications/${billId}`);
    return res.data;
  },
};
