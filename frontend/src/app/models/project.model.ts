import { BackendTask } from "./task.model";

export interface User {
    id: number;
    gitHubID: string;
  }
  
  export interface Project {
    id: number;
    projectName: string;
    projectDescription: string;
    owner: User;
    assignedUsers: User[];
    tasks: BackendTask[];
  }