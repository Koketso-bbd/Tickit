import { Component, OnInit } from '@angular/core';
import { TaskService } from '../../../services/task.service';

@Component({
  selector: 'app-tasks',
  imports: [],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent implements OnInit {
  response: any;

  constructor(private taskService: TaskService) {}

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

  getData() {
    this.taskService.fetchData('tasks').subscribe(
      data => this.response = data
    );
  }

  sendData(){
    const payload = {

    };

    this.taskService.postData('tasks', payload).subscribe(
      data => this.response = data
    );
  }

  getStringifiedResponse(): string {
    return this.response ? JSON.stringify(this.response, null, 2) : 'No data yet';
  }

}
