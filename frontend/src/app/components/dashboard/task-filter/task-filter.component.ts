import { NgIf } from '@angular/common';
import { TaskViewComponent } from '../task-view/task-view.component';
import { Component, OnInit } from '@angular/core';
import { TaskFilterService } from '../../../services/task-filter.service';
import { FormsModule } from '@angular/forms';
import { NgFor, CommonModule } from '@angular/common';
import { switchMap, tap } from 'rxjs';
import {ProjectService} from '../../../services/project.service';
import { Task } from '../../../interfaces/task.interface';
import { Project } from '../../../interfaces/project.interface';

@Component({
  selector: 'app-tasks',
  imports: [NgIf,FormsModule, CommonModule, NgFor],
  templateUrl: './task-filter.component.html',
  styleUrls: ['./task-filter.component.css'],
})

export class TasksFilterComponent implements OnInit {
    task!: Task;
    project!: Project;
    projectId!: number;
    assigneeId!: number;
    priority!: string;
    status!: string;

  tasks: Task[] = [];
  openTasks: Task[] = [];
  closedTasks: Task[] = [];
  filteredTasks: Task[] = [];
  selectedDate: Date | null = null;
  selectedProjectId: number | null = null;
  visibleSection: string = 'taskList'; 
  taskId: number = 120;
  taskViewVisible = false;


  constructor(private taskService: TaskFilterService, private projectService: ProjectService) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.taskService.getUserTasks().pipe(
      switchMap((tasks: Task[]) => {
        this.tasks = tasks;
        this.projectId = tasks[0]?.projectId || 0;
        this.openTasks = tasks.filter((task) => task.statusId >= 1 && task.statusId <= 3);
        this.closedTasks = tasks.filter((task) => task.statusId === 4);
        this.filteredTasks = this.openTasks;
        return this.projectService.getProjects();
      }),
      switchMap((projects: Project[]) => {
        this.project = projects[0];
        return this.projectService.getProjectById(this.projectId);
      }),
      tap((project: Project) => {
        this.project = project;
      })
    ).subscribe(() => {});
  }

  filterByDate(date: Date | null): void {
    if (date !== null) {
      const dateOnly = new Date(date).toISOString().split('T')[0];
      this.filteredTasks = this.tasks.filter((task) => {
        const taskDateOnly = new Date(task.dueDate).toISOString().split('T')[0];
        return taskDateOnly === dateOnly;
      });
    } else {
      this.filteredTasks = [...this.tasks];
    }
  }

  filterByProjectName(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const projectName = inputElement.value;
    if (!projectName || projectName.trim() === '') {
      this.filteredTasks = [...this.tasks];
      return;
    }
  
    this.projectService.getProjects().subscribe({
      next: (projects: any[]) => {
        const project = projects.find((p) => p.projectName.toLowerCase() === projectName.toLowerCase());
        if (project) {
          this.filteredTasks = this.tasks.filter((task) => task.projectId === +project.id);
        } else {
          this.filteredTasks = []; 
        }
      },
      error: (err: any) => console.error('Error fetching projects:', err),
    });
  }
  

  clearFilters(): void {
    this.filteredTasks = [...this.tasks];
    this.selectedDate = null;
    this.selectedProjectId = null;
    this.visibleSection = 'taskList'; 
  }

  showSection(section: string): void {
    this.visibleSection = section;
    if (section === 'taskList') {
      this.filteredTasks = this.openTasks;
    } else if (section === 'closed') {
      this.filteredTasks = this.closedTasks;
    }
  }
  
    openTaskView(): void {
      this.taskViewVisible = true;  
    }
  
   

    getProjectById(id: number) {
      this.projectService
        .getProjectById(id)
        .subscribe(() => {});
    }
}
