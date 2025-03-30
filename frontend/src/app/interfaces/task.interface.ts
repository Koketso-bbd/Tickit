export interface Task {
  assigneeId: number;
  taskName: string;
  taskDescription: string;
  projectId: number;
  projectName?: string; 
  statusId: number;
  statusName?: string;
  priorityId: number;
  dueDate: Date;
  projectLabelIds: string [];
}

export interface UpdateTask {
  assigneeId: number;
  taskName: string;
  taskDescription: string;
  statusId: number;
  priorityId: number;
  dueDate: Date;
}