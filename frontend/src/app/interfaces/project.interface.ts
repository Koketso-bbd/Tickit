import { Task } from "./task.interface";

export interface Project {
  id: string;
  projectName: string;
  projectDescription: string;
  owner: {
    id: number;
    gitHubID: string;
  };
  assignedUsers: [];
  tasks: Task[];
}
