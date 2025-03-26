import { TaskStatus, TaskPriority } from '../enums/task.enums';

export interface BackendTask {
    taskId: number;
    assigneeId: number;
    taskName: string;
    taskDescription?: string;
    dueDate: string;
    priorityId: number;
    projectId: number;
    projectLabelIds: number[];
    statusId?: number; 
  }
  
  export interface Task {
    id: number;
    name: string;
    description?: string;
    dueDate: Date;
    priority: TaskPriority;
    assigneeId: number;
    status: TaskStatus;
    projectId: number;
    projectLabelIds: number[];
  }
