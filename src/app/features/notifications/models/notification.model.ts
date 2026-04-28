export type NotificationCategory = 'Referral' | 'SLA' | 'Subjectivity' | 'Quote' | 'Compliance';
export type NotificationStatus = 'Unread' | 'Read' | 'Dismissed';

export interface Notification {
  notificationId: string;
  userId: string;
  message: string;
  category: NotificationCategory;
  status: NotificationStatus;
  createdDate: string;
}
