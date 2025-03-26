import { Component } from '@angular/core';
import { ProjectService } from '../../../services/project.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-projects',
  imports: [CommonModule, FormsModule],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
})

export class ProjectsComponent {

  projects: any[] = [];
  project: any = null;
  newProject = { projectName: '', projectDescription: ''};
  showForm = false;
  editingProject: any = null;

  constructor(private projectService: ProjectService, private router: Router) {}

  ngOnInit(): void {
    this.fetchProjects();
  }

  fetchProjects() {
    this.projectService.getProjects().subscribe({
      next: (data) => {
        this.projects = data;
      },
      error: (error) => {
        console.error(error);
      }
    })
  }

  viewProject(projectId: number) {
    this.router.navigate([`/dashboard/tasks`, projectId]);
  }

  deleteProject(projectId: number) {
    if(confirm("Are you sure you want to delete this project?")) {
      this.projectService.deleteProject(projectId).subscribe({
        next: (data) => {
          this.project = data;
        },
        error: (error) => {
          console.error(error);
        }
      })
    }
  }
  
  createProject() {
    if (!this.newProject.projectName.trim() || !this.newProject.projectDescription.trim()) {
      return;
    }

    this.projectService.createProject(this.newProject).subscribe({
      next: (data) => {
        console.log('Project created:', data);
        this.projects.push(data);
        this.newProject = { projectName: '', projectDescription: '' };
        this.showForm = false;
      },
      error: (error) => console.error(error)
    });
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  openEditForm(project: any) {
    this.editingProject = { ...project };
  }

  updateProject() {
    if (this.editingProject) {
      this.projectService.updateProject(this.editingProject.id, {
        projectName: this.editingProject.projectName,
        projectDescription: this.editingProject.projectDescription
      }).subscribe(response => {
        const index = this.projects.findIndex(p => p.id === this.editingProject.id);
        if (index !== -1) this.projects[index] = response;
        this.editingProject = null;
      });
    }
  }

  cancelEdit() {
    this.editingProject = null;
  }
}
