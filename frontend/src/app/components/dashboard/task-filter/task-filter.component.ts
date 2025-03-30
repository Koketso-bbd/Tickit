import { NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { TaskFilterService } from '../../../services/task-filter.service';
import { FormsModule } from '@angular/forms';
import { NgFor, CommonModule } from '@angular/common';
import { switchMap, tap } from 'rxjs';
import { ProjectService } from '../../../services/project.service';
import { Task } from '../../../interfaces/task.interface';
import { Project } from '../../../interfaces/project.interface';
import { DatePipe } from '@angular/common';
import { Status } from '../../../enums/status.enum';

@Component({
  selector: 'app-tasks',
  standalone: true, 
  imports: [CommonModule, FormsModule, NgIf, NgFor], 
  templateUrl: './task-filter.component.html',
  styleUrls: ['./task-filter.component.css'],
  providers: [DatePipe] 
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
  visibleSection: string = 'taskList'; 
  taskViewVisible = false;
  selectedTask: Task | null = null;

  constructor(
    private taskService: TaskFilterService,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.taskService.getUserTasks().pipe(
      switchMap((tasks: Task[]) => {
        this.tasks = tasks;
        this.projectId = tasks[0]?.projectId || 0;

        this.openTasks = tasks.filter((task) => task.statusId !== Status.Completed);
        this.closedTasks = tasks.filter((task) => task.statusId === Status.Completed);

        this.filteredTasks = this.openTasks;
        return this.projectService.getProjects();
      }),
      tap((projects: Project[]) => {
        const projectMap = new Map<number, string>();
        projects.forEach((project) => {
          projectMap.set(Number(project.id), project.projectName);
        });

        this.tasks.forEach((task) => {
          task.projectName = projectMap.get(task.projectId) || 'Unknown Project';
        });
        this.tasks.forEach((task) => {
          task.statusName = Status[task.statusId] || 'Unknown Status';
        });
        
      })
    ).subscribe();
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  filterByDate(date: Date | null): void {
    if (date !== null) {
      const dateOnly = this.formatDate(new Date(date));
      this.filteredTasks = this.tasks.filter((task) => {
        const taskDateOnly = this.formatDate(new Date(task.dueDate));
        return taskDateOnly === dateOnly;
      });
    } else {
      this.filteredTasks = this.tasks.filter(task => task.statusId !== Status.Completed);
    }
  }
  
  

  filterByProjectName(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    const projectName = inputElement.value.trim();

    if (!projectName) {
      this.filteredTasks = this.tasks.filter(task => task.statusId !== Status.Completed);
      return;
    }

    this.projectService.getProjects().subscribe({
      next: (projects: Project[]) => {
        const project = projects.find(p => p.projectName.toLowerCase() === projectName.toLowerCase());

        if (project) {
          this.filteredTasks = this.tasks.filter(
            (task) => task.projectId === +project.id && task.statusId !== Status.Completed
          );
        } else {
          this.filteredTasks = [];
        }
      },
      error: (err: any) => console.error('Error fetching projects:', err),
    });
  }

  clearFilters(): void {
    this.filteredTasks = this.tasks.filter(task => task.statusId !== Status.Completed);
    this.selectedDate = null;
    this.visibleSection = 'taskList'; 
  }

  openTaskView(): void {
    this.taskViewVisible = true;
  }

  getProjectById(id: number): void {
    this.projectService.getProjectById(id).subscribe({
      next: (project: Project) => {
        this.project = project;
      },
      error: (err: any) => console.error('Error fetching project:', err),
    });
  }

  showTaskDetails(task: Task): void {
    this.selectedTask = task;
    this.taskViewVisible = true;
  }

  closeTaskView(): void {
    this.taskViewVisible = false;
    this.selectedTask = null;
  }
}