import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TaskViewService } from '../../../services/task-view.service';
import { Task } from '../../../interfaces/task.interface';
import { Project } from '../../../interfaces/project.interface';
import { ProjectsService } from '../../../services/projects.service';
import { switchMap, tap } from 'rxjs';
import { UserService } from '../../../services/user.service';
import { User } from '../../../interfaces/user.interface';
import { Priority } from '../../../enums/priority.enum';
import { Status } from '../../../enums/status.enum';

@Component({
  selector: 'app-task-view',
  imports: [NgFor, DatePipe],
  templateUrl: './task-view.component.html',
  styleUrl: './task-view.component.css'
})
export class TaskViewComponent implements OnInit {
  @Input() taskId!: number;
  @Output() closePopup = new EventEmitter<void>();
  task!: Task;
  project!: Project;
  projectId!: number;
  assignee!: User;
  assigneeId!: number;
  priority!: string;
  status!: string;

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

  closeTaskView(): void {
    this.closePopup.emit();  
  }
}
