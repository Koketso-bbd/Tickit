export interface Task {
    id: string;
    name: string;
    description?: string;
    dueDate?: Date;
    priority?: 'low' | 'medium' | 'high' | 'urgent';
    assignee?: string;
    status: 'unconfirmed' | 'todo' | 'in-progress' | 'completed';
    acknowledged: boolean;
  }