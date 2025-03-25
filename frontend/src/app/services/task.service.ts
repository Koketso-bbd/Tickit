import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Task } from '../models/task.model';
import { v4 as uuidv4 } from 'uuid';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private tasks: Task[] = [
    {
      id: uuidv4(),
      name: 'Example Task',
      description: 'This is an example task',
      dueDate: new Date(2025, 3, 30),
      priority: 'medium',
      assignee: 'John Doe',
      status: 'unconfirmed',
      acknowledged: false
    }
  ];
  
  private tasksSubject = new BehaviorSubject<Task[]>(this.tasks);
  
  getTasks(): Observable<Task[]> {
    return this.tasksSubject.asObservable();
  }
  
  addTask(task: Omit<Task, 'id' | 'status' | 'acknowledged'>): void {
    const newTask: Task = {
      ...task,
      id: uuidv4(),
      status: 'unconfirmed',
      acknowledged: false
    };
    
    this.tasks = [...this.tasks, newTask];
    this.tasksSubject.next(this.tasks);
  }
  
  updateTask(updatedTask: Task): void {
    this.tasks = this.tasks.map(task => 
      task.id === updatedTask.id ? updatedTask : task
    );
    this.tasksSubject.next(this.tasks);
  }
  
  acknowledgeTask(taskId: string): void {
    const task = this.tasks.find(t => t.id === taskId);
    if (task) {
      task.acknowledged = true;
      task.status = 'todo';
      this.tasksSubject.next(this.tasks);
    }
  }
  
  moveTask(taskId: string, newStatus: Task['status']): void {
    const task = this.tasks.find(t => t.id === taskId);
    if (task) {
      task.status = newStatus;
      this.tasksSubject.next(this.tasks);
    }
  }
}