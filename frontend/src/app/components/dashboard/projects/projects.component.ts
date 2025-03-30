import { Component } from '@angular/core';
import { ProjectService } from '../../../services/project.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../../shared/confirmation-dialog/confirmation-dialog.component';

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

  formSubmitted: boolean = false;

  constructor(
    private projectService: ProjectService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) { }

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
    this.router.navigate([`/dashboard/projects`, projectId]);
  }

  deleteProject(projectId: number) {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Project',
        message: 'Are you sure you want to delete this project?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.projectService.deleteProject(projectId).subscribe({
          next: (data) => {
            this.project = data;
            this.fetchProjects();
            this.snackBar.open('Project deleted successfully!', 'Close', { duration: 3000 });
          },
          error: (error) => {
            console.error(error);
            this.snackBar.open('Failed to delete project.', 'Close', { duration: 3000 });
          }
        });
      }
    });
  }

  createProject() {
    this.formSubmitted = true;
    if (!this.newProject.projectName.trim()) {
      this.snackBar.open('Please fill in the required fields.', 'Close', { duration: 3000 });
      return;
    }

    this.projectService.createProject(this.newProject).subscribe({
      next: (data) => {
        console.log('Project created:', data);
        this.projects.push(data);
        this.newProject = { projectName: '', projectDescription: '' };
        this.showForm = false;
        this.snackBar.open('Project created successfully!', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error(error);
        this.snackBar.open('Failed to create project.', 'Close', { duration: 3000 });
      }
    });
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  openEditForm(project: any) {
    this.editingProject = { ...project };
  }

  updateProject() {
    this.formSubmitted = true;

    if (this.editingProject) {
      this.projectService.updateProject(this.editingProject.id, {
        projectName: this.editingProject.projectName,
        projectDescription: this.editingProject.projectDescription
      }).subscribe({
        next: (response) => {
          const index = this.projects.findIndex(p => p.id === this.editingProject.id);
          if (index !== -1) this.projects[index] = response;
          this.editingProject = null;
          this.snackBar.open('Project updated successfully!', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error(error);
          this.snackBar.open('Failed to update project.', 'Close', { duration: 3000 });
        }
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
    this.formSubmitted = true;
    if (!this.userToAdd.userId || !this.userToAdd.roleId || !this.selectedProject) {
      this.snackBar.open('Please fill in the required fields.', 'Close', { duration: 3000 });
      return;
    }

    this.projectService.addUserToProject(
      this.userToAdd.userId,
      this.selectedProject.id,
      this.userToAdd.roleId
    ).subscribe({
      next: (response) => {
        console.log('User added to project:', response);
        this.fetchProjects();
        this.closeAddUserForm();
        this.snackBar.open('User added successfully!', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error(error);
        this.snackBar.open('Failed to add user.', 'Close', { duration: 3000 });
      }
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
    this.formSubmitted = true;
    if (!this.userToRemove.userId || !this.selectedProject) {
      this.snackBar.open('Please fill in the required fields.', 'Close', { duration: 3000 });
      return;
    }

    this.projectService.removeUserFromProject(
      this.userToRemove.userId,
      this.selectedProject.id
    ).subscribe({
      next: (response) => {
        console.log('User removed from project:', response);
        this.fetchProjects();
        this.closeRemoveUserForm();
        this.snackBar.open('User removed successfully!', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error(error);
        this.snackBar.open('Failed to remove user.', 'Close', { duration: 3000 });
      }
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
    this.formSubmitted = true;
    if (!this.userRoleUpdate.userId || !this.userRoleUpdate.newRoleId || !this.selectedProject) {
      this.snackBar.open('Please fill in the required fields.', 'Close', { duration: 3000 });
      return;
    }

    this.projectService.updateUserRole(
      this.userRoleUpdate.userId,
      this.selectedProject.id,
      this.userRoleUpdate.newRoleId
    ).subscribe({
      next: (response) => {
        console.log('User role updated:', response);
        this.fetchProjects();
        this.closeUpdateRoleForm();
        this.snackBar.open('User role updated successfully!', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error(error);
        this.snackBar.open('Failed to update user role.', 'Close', { duration: 3000 });
      }
    });
  }
}