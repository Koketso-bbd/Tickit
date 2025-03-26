import { Component, OnInit } from '@angular/core';
import { TaskService } from '../../../services/tasks/task.service';
import { Task } from '../../../models/task.model';
import { User } from '../../../models/project.model';
import { TaskStatus, TaskPriority } from '../../../enums/task.enums';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { map, Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { NgIf } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tasks',
  imports: [NgIf ,CommonModule, ReactiveFormsModule],
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css'],
})

export class TasksComponent implements OnInit {
  response: any;
  TaskStatus = TaskStatus;
  tasks$: Observable<Task[]>;
  todoTasks$: Observable<Task[]>;
  inProgressTasks$: Observable<Task[]>;
  inReviewTasks$: Observable<Task[]>;
  completedTasks$: Observable<Task[]>;
  
  assignedUsers: User[] = [];
  
  taskForm: FormGroup;
  editingTask: Task | null = null;
  showTaskForm = false;
  projectId: number | null = null;
  
  priorities = Object.entries(TaskPriority)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({ 
      value: value, 
      label: key 
    }));
  
  constructor(
    private taskService: TaskService,
    private fb: FormBuilder,
    private route: ActivatedRoute
  ) {
    this.tasks$ = this.taskService.tasks$;
    
    this.todoTasks$ = this.taskService.getTasksByStatus(TaskStatus.ToDo);
    this.inProgressTasks$ = this.taskService.getTasksByStatus(TaskStatus.InProgress);
    this.inReviewTasks$ = this.taskService.getTasksByStatus(TaskStatus.InReview);
    this.completedTasks$ = this.taskService.getTasksByStatus(TaskStatus.Completed);
    
    this.taskForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      dueDate: [null],
      priority: [TaskPriority.Low],
      assigneeId: [null]
    });
  }
  
  ngOnInit(): void {
    this.projectId = +this.route.snapshot.paramMap.get('projectId')!;
    this.taskService.fetchProject(this.projectId).subscribe({
      next: () => {
        this.assignedUsers = this.taskService.getAssignedUsers();
      },
      error: (err) => console.error('Error loading project', err)
    });
  }
  
  openTaskForm(task?: Task): void {
    if (task) {
      this.editingTask = task;
      this.taskForm.patchValue({
        name: task.name,
        description: task.description || '',
        dueDate: task.dueDate,
        priority: task.priority,
        assigneeId: task.assigneeId
      });
    } else {
      this.editingTask = null;
      this.taskForm.reset({
        priority: TaskPriority.Medium,
        assigneeId: null
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
    
    const taskData: Omit<Task, 'id'> = {
      name: formValue.name,
      description: formValue.description,
      dueDate: formValue.dueDate,
      priority: formValue.priority,
      assigneeId: formValue.assigneeId,
      projectId: 29, 
      status: this.editingTask ? 
        this.editingTask.status : 
        TaskStatus.ToDo,
      projectLabelIds: []
    };
    
    if (this.editingTask) {
      this.taskService.updateTask({
        ...taskData,
        id: this.editingTask.id
      }).subscribe({
        next: () => this.closeTaskForm(),
        error: (err) => console.error('Error updating task', err)
      });
    } else {
      this.taskService.addTask(taskData).subscribe({
        next: () => this.closeTaskForm(),
        error: (err) => console.error('Error creating task', err)
      });
    }
  }

  deleteTask(taskId: number): void {
    if (confirm("Are you sure you want to delete this task?")) {
        this.taskService.deleteTask(taskId).subscribe({
            next: () => {
                this.tasks$ = this.tasks$.pipe(
                    map(tasks => tasks.filter(task => task.id !== taskId))
                );
            },
            error: (error) => {
                console.error("Error deleting task:", error);
            }
        });
    }
}

  moveTask(task: Task, newStatus: TaskStatus): void {
    const previousStatus = task.id;

    this.taskService.moveTask(task.id, newStatus);

    this.taskService.updateTaskStatus(task.id, { "statusId": newStatus }).subscribe({
        next: (data) => {
            console.log("Task updated successfully", data);
        },
        error: (error) => {
            console.error("Error updating task status", error);
            task.id = previousStatus;
        }
    });
  }
  
  allowDrop(event: DragEvent): void {
    event.preventDefault();
  }
  
  dragStart(event: DragEvent, task: Task): void {
    if (event.dataTransfer) {
      event.dataTransfer.setData('taskId', task.id.toString());
    }
  }
  
  dropTask(event: DragEvent, status: TaskStatus): void {
    event.preventDefault();
    if (event.dataTransfer) {
      const taskId = parseInt(event.dataTransfer.getData('taskId'), 10);
      this.moveTask({ id: taskId } as Task, status);
    }
  }

  acknowledgeTask(taskId: number): void {
    this.taskService.acknowledgeTask(taskId);
  }

  getAssignedUserName(assigneeId: number): string {
    const user = this.assignedUsers.find(user => user.id === assigneeId);
    return user ? user.gitHubID : 'Unassigned';
  }

  isValidDate(date: any): boolean {
    return date && !isNaN(new Date(date).getTime());
  } 
  
  getPriorityLabel(priority: number): string {
    return TaskPriority[priority] || 'Unknown';
  }
  
  
}