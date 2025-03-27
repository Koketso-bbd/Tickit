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
  newProject = { projectName: '', projectDescription: '' };
  
  showForm = false;
  editingProject: any = null;
  showAddUserForm = false;
  showRemoveUserForm = false;
  showUpdateRoleForm = false;
  
  selectedProject: any = null;
  userToAdd = { userId: null as number | null, roleId: null as number | null };
  userToRemove = { userId: null as number | null };
  userRoleUpdate = { userId: null as number | null, newRoleId: null as number | null };

  constructor(private projectService: ProjectService, private router: Router) { }

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
    });
  }

  viewProject(projectId: number) {
    this.router.navigate([`/dashboard/tasks`, projectId]);
  }

  deleteProject(projectId: number) {
    if (confirm("Are you sure you want to delete this project?")) {
      this.projectService.deleteProject(projectId).subscribe({
        next: (data) => {
          this.project = data;
          this.fetchProjects();
        },
        error: (error) => {
          console.error(error);
        }
      });
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

  openAddUserForm(project: any) {
    this.selectedProject = project;
    this.userToAdd = { userId: null, roleId: null };
    this.showAddUserForm = true;
  }

  closeAddUserForm() {
    this.showAddUserForm = false;
    this.selectedProject = null;
  }

  addUserToProject() {
    if (!this.userToAdd.userId || !this.userToAdd.roleId || !this.selectedProject) return;

    this.projectService.addUserToProject(
      this.userToAdd.userId, 
      this.selectedProject.id, 
      this.userToAdd.roleId
    ).subscribe({
      next: (response) => {
        console.log('User added to project:', response);
        this.fetchProjects();
        this.closeAddUserForm();
      },
      error: (error) => console.error(error)
    });
  }

  openRemoveUserForm(project: any) {
    this.selectedProject = project;
    this.userToRemove = { userId: null };
    this.showRemoveUserForm = true;
  }

  closeRemoveUserForm() {
    this.showRemoveUserForm = false;
    this.selectedProject = null;
  }

  removeUserFromProject() {
    if (!this.userToRemove.userId || !this.selectedProject) return;

    this.projectService.removeUserFromProject(
      this.userToRemove.userId, 
      this.selectedProject.id
    ).subscribe({
      next: (response) => {
        console.log('User removed from project:', response);
        this.fetchProjects();
        this.closeRemoveUserForm();
      },
      error: (error) => console.error(error)
    });
  }

  openUpdateRoleForm(project: any) {
    this.selectedProject = project;
    this.userRoleUpdate = { userId: null, newRoleId: null };
    this.showUpdateRoleForm = true;
  }

  closeUpdateRoleForm() {
    this.showUpdateRoleForm = false;
    this.selectedProject = null;
  }

  updateUserRole() {
    if (!this.userRoleUpdate.userId || !this.userRoleUpdate.newRoleId || !this.selectedProject) return;

    this.projectService.updateUserRole(
      this.userRoleUpdate.userId, 
      this.selectedProject.id, 
      this.userRoleUpdate.newRoleId
    ).subscribe({
      next: (response) => {
        console.log('User role updated:', response);
        this.fetchProjects();
        this.closeUpdateRoleForm();
      },
      error: (error) => console.error(error)
    });
  }
}