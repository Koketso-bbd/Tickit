import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TaskViewService } from '../../../task-view.service';
import { Task } from '../../../task.interface';
import { Project } from '../../../project.interface';
import { ProjectsService } from '../../../projects.service';
import { switchMap, tap } from 'rxjs';
import { UserService } from '../../../user.service';
import { User } from '../../../user.interface';

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

  closeTaskView(): void {
    this.closePopup.emit();  
  }
}
