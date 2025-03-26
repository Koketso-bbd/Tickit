import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Project, User } from '../models/project.model';
import { Task, BackendTask } from '../models/task.model';
import { TaskStatus, TaskPriority } from '../enums/task.enums';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private apiUrl = 'https://localhost:7151/api'; 
  
  private projectSubject = new BehaviorSubject<Project | null>(null);
  currentProject$ = this.projectSubject.asObservable();
  
  private tasksSubject = new BehaviorSubject<Task[]>([]);
  tasks$ = this.tasksSubject.asObservable();

  constructor(private http: HttpClient) {}

  private getHeaders(): { [header: string]: string } {
    const token = localStorage.getItem("jwt")
    return {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    };
  }

  fetchProject(projectId: number): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/projects/${projectId}`, { 
      headers: this.getHeaders() 
    }).pipe(
      tap(project => {
        this.projectSubject.next(project);
        
        const convertedTasks = project.tasks.map(this.mapBackendToFrontendTask);
        this.tasksSubject.next(convertedTasks);
      })
    );
  }

  private mapBackendToFrontendTask(backendTask: BackendTask): Task {
    console.log(backendTask.statusId);
    return {
      id: backendTask.taskId,
      name: backendTask.taskName,
      description: backendTask.taskDescription,
      dueDate: new Date(backendTask.dueDate),
      priority: backendTask.priorityId as TaskPriority,
      assigneeId: backendTask.assigneeId,
      status: backendTask.statusId as TaskStatus,
      projectId: backendTask.projectId,
      projectLabelIds: backendTask.projectLabelIds || []
    };
  }

  private mapFrontendToBackendTask(task: Task): BackendTask {
    return {
      taskId: task.id,
      assigneeId: task.assigneeId,
      taskName: task.name,
      taskDescription: task.description,
      dueDate: task.dueDate,
      priorityId: task.priority,
      projectId: task.projectId,
      projectLabelIds: task.projectLabelIds || [],
      statusId: task.status
    };
  }

  getTasksByStatus(status: TaskStatus): Observable<Task[]> {
    return this.tasks$.pipe(
      map(tasks => tasks.filter(task => task.status === status))
    );
  }

  addTask(task: Omit<Task, 'id'>): Observable<Task> {
    const backendTask = this.mapFrontendToBackendTask({
      ...task,
      id: 0
    });

    return this.http.post<BackendTask>(`${this.apiUrl}/tasks`, backendTask, { 
      headers: this.getHeaders() 
    }).pipe(
      map(createdTask => {
        const mappedTask = this.mapBackendToFrontendTask(createdTask);
        const currentTasks = this.tasksSubject.value;
        this.tasksSubject.next([...currentTasks, mappedTask]);
        return mappedTask;
      })
    );
  }

  updateTask(task: Task): Observable<Task> {
    const backendTask = this.mapFrontendToBackendTask(task);

    return this.http.put<BackendTask>(`${this.apiUrl}/tasks/${task.id}`, backendTask, { 
      headers: this.getHeaders() 
    }).pipe(
      map(updatedTask => {
        const mappedTask = this.mapBackendToFrontendTask(updatedTask);
        const currentTasks = this.tasksSubject.value;
        const updatedTasks = currentTasks.map(t => 
          t.id === mappedTask.id ? mappedTask : t
        );
        this.tasksSubject.next(updatedTasks);
        return mappedTask;
      })
    );
  }

  deleteTask(taskId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/tasks/${taskId}`, { headers: this.getHeaders() });
  }

    moveTask(taskId: number, newStatus: TaskStatus): void {
      const currentTasks = this.tasksSubject.value;
    
      const updatedTasks = currentTasks.map(t =>
        t.id === taskId ? { ...t, status: newStatus } : t
      );
    
      this.tasksSubject.next(updatedTasks);
   }

   updateData(taskId: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/tasks/${taskId}`, data, { headers: this.getHeaders() });
  }
  
  acknowledgeTask(taskId: number): void {
    const tasks = this.tasksSubject.getValue(); 
    const updatedTasks = tasks.map(task =>
      task.id === taskId ? { ...task, acknowledged: true, status: TaskStatus.ToDo } : task
    );
  
    this.tasksSubject.next(updatedTasks); 
  }
  
    getAssignedUsers(): User[] {
    return this.projectSubject.value?.assignedUsers || [];
  }
}