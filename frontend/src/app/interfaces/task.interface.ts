export interface Task {
  //id: string;
  assigneeId: number;
  taskName: string;
  taskDescription: string;
  projectId: number;
  statusId: number;
  priorityId: number;
  dueDate: Date;
  projectLabelIds: string [];
}
