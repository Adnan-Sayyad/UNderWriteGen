export type NotificationCategory = 'Referral' | 'SLA' | 'Subjectivity' | 'Quote' | 'Compliance';
export type NotificationStatus   = 'Unread' | 'Read' | 'Dismissed';

export interface Notification {
  notificationID:  string;
  recipientEmail:  string;
  senderEmail:     string;
  message:         string;
  category:        NotificationCategory;
  status:          NotificationStatus;
  createdDate:     string;
}
