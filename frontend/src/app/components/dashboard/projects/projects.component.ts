import { Component } from '@angular/core';
import { ProjectService } from '../../../services/project.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-projects',
  imports: [CommonModule],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
})

export class ProjectsComponent {
  projects: any[] = [];
  constructor(private projectService: ProjectService) { }

  ngOnInit(): void {
    this.fetchProjects();
  }

  fetchProjects() {
    this.projectService.getProjects().subscribe({
      next: (data) => {
        this.projects = data;
        console.log(this.projects);
      },
      error: (error) => {
        console.error(error);
      }
    })
  }
}
