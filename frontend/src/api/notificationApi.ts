import { notificationClient } from './httpClient';
import type { NotificationResponse } from './types';

export const notificationApi = {
  list: () => notificationClient.get<NotificationResponse[]>('/api/notifications/'),
  markAsRead: (notificationId: string) =>
    notificationClient.put<void>(`/api/notifications/${notificationId}/read`),
};
