import { DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TaskViewService } from '../../task-view.service';
import { Task } from '../../task.interface';
import { Project } from '../../project.interface';
import { ProjectsService } from '../../projects.service';
import { switchMap } from 'rxjs';

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

  constructor(
    private taskViewService: TaskViewService,
    private projectService: ProjectsService,
  ) { }

  ngOnInit(): void {
    this.getTaskById(this.taskId)
  }

  getTaskById(id: number) {
    this.taskViewService.getTaskById(id).pipe(
      switchMap((task: Task) => {
        this.task = task;
        return this.projectService.getProjectById(task.projectId);
      })
    ).subscribe(
      (project: Project) => {
        this.project = project;
        alert(JSON.stringify(this.project, null, 2));
      });
  }

  getProjectById(id: number) {
    this.projectService
      .getProjectById(id)
      .subscribe((project) => {
        this.project = project
        alert('asd');
      });
  }

  closeTaskView(): void {
    this.closePopup.emit();  
  }
}
