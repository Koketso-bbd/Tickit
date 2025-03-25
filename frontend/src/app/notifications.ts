export interface Notification {
    id: string;
    userId: string;
    projectId: string;
    taskId: string;
    notificationTypeId: string;
    message: string;
    isRead: boolean;
    createdAt: string;
  }
  