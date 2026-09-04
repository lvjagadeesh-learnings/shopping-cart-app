import { useState } from 'react';
import { useMarkNotificationRead, useNotifications } from '../hooks/useNotifications';
export function NotificationsBell() {
  const [open, setOpen] = useState(false);
  const {
    data: notifications
  } = useNotifications();
  const markAsRead = useMarkNotificationRead();
  const unreadCount = notifications?.filter(n => !n.isRead).length ?? 0;
  return <div className="relative">
      <button onClick={() => setOpen(value => !value)} className="relative flex items-center gap-1" aria-label="Notifications">
        <span aria-hidden>🔔</span>
        {unreadCount > 0 && <span className="absolute -right-3 -top-2 rounded-full bg-white px-1.5 text-xs font-bold text-brand-600">
            {unreadCount}
          </span>}
      </button>

      {open && <div className="absolute right-0 z-10 mt-2 w-80 rounded border border-gray-200 bg-white text-gray-800 shadow-lg">
          <div className="max-h-96 overflow-y-auto">
            {!notifications || notifications.length === 0 ? <p className="p-4 text-sm text-gray-500">No notifications yet.</p> : <ul className="divide-y divide-gray-100">
                {notifications.map(notification => <li key={notification.id} className={`p-3 text-sm ${notification.isRead ? '' : 'bg-brand-50'}`}>
                    <p className="font-medium">{notification.title}</p>
                    <p className="text-gray-600">{notification.message}</p>
                    <div className="mt-1 flex items-center justify-between text-xs text-gray-400">
                      <span>{new Date(notification.createdAtUtc).toLocaleString()}</span>
                      {!notification.isRead && <button onClick={() => markAsRead.mutate(notification.id)} className="text-brand-600 hover:underline">
                          Mark read
                        </button>}
                    </div>
                  </li>)}
              </ul>}
          </div>
        </div>}
    </div>;
}
