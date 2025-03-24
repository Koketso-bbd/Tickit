import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProjectsComponent } from './components/projects/projects.component';
import { TasksComponent } from './components/tasks/tasks.component';

export const routes: Routes = [
    {path: '', component: LandingComponent},
    {path: 'dashboard', component: DashboardComponent, children: [
        {path: '', component: DashboardComponent},
        {path: 'projects', component: ProjectsComponent},
        {path: 'tasks', component: TasksComponent}
    ]},
    {path: '**', component: NotFoundComponent}
];
