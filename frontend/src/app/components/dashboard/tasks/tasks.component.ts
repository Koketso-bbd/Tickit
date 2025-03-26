import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { TaskViewComponent } from '../task-view/task-view.component';

@Component({
  selector: 'app-tasks',
  imports: [TaskViewComponent, NgIf],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent {
  taskId: number = 120;
  taskViewVisible = false;

  openTaskView(): void {
    this.taskViewVisible = true;  
  }

  closeTaskView(): void {
    this.taskViewVisible = false;  
  }
}
