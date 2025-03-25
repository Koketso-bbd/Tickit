import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from '../../../services/project.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-project-detail',
  imports: [CommonModule],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.css'
})
export class ProjectDetailComponent implements OnInit {
  
  project: any = null;
  constructor(private route: ActivatedRoute, private projectService: ProjectService, private router: Router) {}

  ngOnInit(): void {
    const projectId = this.route.snapshot.paramMap.get('id');
    if (projectId) {
      this.fetchProjectDetails(+projectId);
    }
  }

  fetchProjectDetails(projectId: number) {
    this.projectService.getProjectById(projectId).subscribe({
      next: (data) => {
        this.project = data;
        console.log('Project Details:', this.project);
      },
      error: (error) => {
        console.error('Error fetching project:', error);
      }
    });
  }

  goBack() {
    this.router.navigate(['/dashboard/projects']);
  }
}
