import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TaskViewService } from '../../../services/task-view.service';
import { Task, UpdateTask } from '../../../interfaces/task.interface';
import { Project } from '../../../interfaces/project.interface';
import { ProjectsService } from '../../../services/projects.service';
import { switchMap, tap } from 'rxjs';
import { UserService } from '../../../services/user.service';
import { User } from '../../../interfaces/user.interface';
import { Priority } from '../../../enums/priority.enum';
import { Status } from '../../../enums/status.enum';
import { FormsModule, NgModel } from '@angular/forms';

@Component({
  selector: 'app-task-view',
  imports: [NgFor, DatePipe, FormsModule, NgIf],
  templateUrl: './task-view.component.html',
  styleUrl: './task-view.component.css'
})
export class TaskViewComponent implements OnInit {
  @Input() taskId!: number;
  @Input() task!: Task;
  @Input() project!: Project;
  @Input() assignee!: User;
  @Input() priorityId!: Priority;
  @Input() statusId!: Status;
  @Output() closePopup = new EventEmitter<void>();
  projectId!: number;
  assigneeId!: number;
  priority!: string;
  status!: string;
  Status = Status;
  Priority = Priority;
  isEditMode = false;

  originalTask!: Task;
  originalStatusId!: Status;
  originalPriorityId!: Priority;

  constructor(
    private taskViewService: TaskViewService,
    private projectService: ProjectsService,
    private userService: UserService,
  ) { }

  ngOnInit(): void {
    this.getTaskById(this.taskId)
  }

  getTaskById(id: number) {
    this.taskViewService.getTaskById(id).pipe(
      switchMap((task: Task) => {
        this.task = task;
        this.projectId = task.projectId;
        this.assigneeId = task.assigneeId;
        this.priorityId = task.priorityId;
        this.statusId = task.statusId;
        this.priority = this.getPriority(task.priorityId);
        this.status = this.getStatus(task.statusId);
        return this.projectService.getProjectById(this.projectId);
      }),
      switchMap((project: Project) => {
        this.project = project;
        return this.userService.getUsertById(this.assigneeId)
      }),
      tap((assignee: User) => {
        this.assignee = assignee;
      })
    ).subscribe(() => {});
  }

  updateTask(id: number, updatedTask: UpdateTask) {
    this.taskViewService.updateTask(id, updatedTask).subscribe((response) => {
      console.log('Project updated successfully:', response);
    });
  }

  getProjectById(id: number) {
    this.projectService
      .getProjectById(id)
      .subscribe(() => {});
  }

  getUserById(id: number) {
    this.userService
      .getUsertById(id)
      .subscribe(() => { });
  }

  getPriority(priorityId: number): string {
    return Priority[priorityId];
  }

  getStatus(statusId: number): string {
    return Status[statusId];
  }

  toggleEditMode() {
    if (!this.isEditMode) {
      this.originalTask = JSON.parse(JSON.stringify(this.task));
      this.originalPriorityId = this.priorityId;
      this.originalStatusId = this.statusId;
    }
    this.isEditMode = !this.isEditMode;
    this.saveTaskData();
  }

  cancelEdit() {
    this.task = JSON.parse(JSON.stringify(this.originalTask));
    this.priorityId = this.originalPriorityId;
    this.statusId = this.originalStatusId;

    this.toggleEditMode();
  }

  saveTaskData() {
    const taskData: UpdateTask = {
      assigneeId: this.assigneeId,
      taskName: this.task.taskName,
      taskDescription: this.task.taskDescription,
      dueDate: this.task.dueDate,
      priorityId: this.priorityId,
      statusId: this.statusId
    };
    this.priority = this.getPriority(this.priorityId);
    this.status = this.getStatus(this.statusId);

    this.updateTask(this.taskId, taskData);
  }

  getStatusOptions(): { value: number, label: string }[] {
    return [
      { value: Status.Unconfirmed, label: 'Unconfirmed' },
      { value: Status.ToDo, label: 'To Do' },
      { value: Status.InProgress, label: 'In Progress' },
      { value: Status.Completed, label: 'Completed' }
    ];
  }

  getPriorityOptions(): { value: number, label: string }[] {
    return [
      { value: Priority.Low, label: 'Low' },
      { value: Priority.Medium, label: 'Medium' },
      { value: Priority.High, label: 'High' },
      { value: Priority.Urgent, label: 'Urgent' }
    ];
  }

  closeTaskView(): void {
    this.closePopup.emit();  
  }
}
