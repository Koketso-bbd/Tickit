import { Component, OnInit } from '@angular/core';
import { TaskService } from '../../../services/task.service';
import { Task } from '../../../models/task.model';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { FormBuilder, ReactiveFormsModule , FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-task-board',
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css'],
  imports: [CommonModule, ReactiveFormsModule]
})
export class TasksComponent implements OnInit {
  tasks$: Observable<Task[]>;
  unconfirmedTasks$: Observable<Task[]>;
  todoTasks$: Observable<Task[]>;
  inProgressTasks$: Observable<Task[]>;
  completedTasks$: Observable<Task[]>;
  
  taskForm: FormGroup;
  editingTask: Task | null = null;
  showTaskForm = false;
  
  priorities = [
    { value: 'low', label: 'Low' },
    { value: 'medium', label: 'Medium' },
    { value: 'high', label: 'High' },
    { value: 'urgent', label: 'Urgent' }
  ];
  
  constructor(
    private taskService: TaskService,
    private fb: FormBuilder
  ) {
    this.tasks$ = this.taskService.getTasks();
    
    this.unconfirmedTasks$ = this.tasks$.pipe(
      map(tasks => tasks.filter(task => task.status === 'unconfirmed'))
    );
    
    this.todoTasks$ = this.tasks$.pipe(
      map(tasks => tasks.filter(task => task.status === 'todo'))
    );
    
    this.inProgressTasks$ = this.tasks$.pipe(
      map(tasks => tasks.filter(task => task.status === 'in-progress'))
    );
    
    this.completedTasks$ = this.tasks$.pipe(
      map(tasks => tasks.filter(task => task.status === 'completed'))
    );
    
    this.taskForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      dueDate: [null],
      priority: ['medium'],
      assignee: ['John Doe'] 
    });
  }
  
  ngOnInit(): void {}
  
  openTaskForm(task?: Task): void {
    if (task) {
      this.editingTask = task;
      this.taskForm.patchValue({
        name: task.name,
        description: task.description || '',
        dueDate: task.dueDate,
        priority: task.priority || 'medium',
        assignee: task.assignee || 'John Doe'
      });
    } else {
      this.editingTask = null;
      this.taskForm.reset({
        priority: 'medium',
        assignee: 'John Doe'
      });
    }
    this.showTaskForm = true;
  }
  
  closeTaskForm(): void {
    this.showTaskForm = false;
    this.editingTask = null;
  }
  
  saveTask(): void {
    if (this.taskForm.invalid) return;
    
    const formValue = this.taskForm.value;
    
    if (this.editingTask) {
      // Update existing task
      const updatedTask: Task = {
        ...this.editingTask,
        name: formValue.name,
        description: formValue.description,
        dueDate: formValue.dueDate,
        priority: formValue.priority,
        assignee: formValue.assignee
      };
      this.taskService.updateTask(updatedTask);
    } else {
      // Create new task
      this.taskService.addTask({
        name: formValue.name,
        description: formValue.description,
        dueDate: formValue.dueDate,
        priority: formValue.priority,
        assignee: formValue.assignee
      });
    }
    
    this.closeTaskForm();
  }
  
  acknowledgeTask(taskId: string): void {
    this.taskService.acknowledgeTask(taskId);
  }
  
  moveTask(task: Task, newStatus: Task['status']): void {
    this.taskService.moveTask(task.id, newStatus);
  }
  
  allowDrop(event: DragEvent): void {
    event.preventDefault();
  }
  
  dragStart(event: DragEvent, task: Task): void {
    if (event.dataTransfer) {
      event.dataTransfer.setData('taskId', task.id);
    }
  }
  
  dropTask(event: DragEvent, status: Task['status']): void {
    event.preventDefault();
    if (event.dataTransfer) {
      const taskId = event.dataTransfer.getData('taskId');
      this.taskService.moveTask(taskId, status);
    }
  }
}