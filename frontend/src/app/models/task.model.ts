import { Priority } from '../enums/priority.enum';
import { Status } from '../enums/status.enum';

export interface BackendTask {
    taskId: number;
    assigneeId: number;
    taskName: string;
    taskDescription?: string;
    dueDate: Date;
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
    priority: Priority;
    assigneeId: number;
    status: Status;
    projectId: number;
    projectLabelIds: number[];
  }
