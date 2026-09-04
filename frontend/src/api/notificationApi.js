import { notificationClient } from './httpClient';
export const notificationApi = {
  list: () => notificationClient.get('/api/notifications/'),
  markAsRead: notificationId => notificationClient.put(`/api/notifications/${notificationId}/read`)
};
