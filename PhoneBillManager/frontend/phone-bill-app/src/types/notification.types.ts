export interface NotificationDto {
  notificationId: number;
  lineId: number;
  recipientName?: string;
  recipientContact: string;
  channel: "SMS" | "WhatsApp";
  status: "Pending" | "Sent" | "Delivered" | "Failed";
  sentAt?: string;
  errorMessage?: string;
}

export interface SendNotificationRequest {
  channel: "SMS" | "WhatsApp";
}
