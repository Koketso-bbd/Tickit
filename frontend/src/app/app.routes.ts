import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProjectsComponent } from './components/dashboard/projects/projects.component';
import { TasksComponent } from './components/dashboard/tasks/tasks.component';
import { NotificationsComponent } from './components/dashboard/notifications/notifications.component';

export const routes: Routes = [
    {path: '', component: LandingComponent},
    {path: 'dashboard', component: DashboardComponent, children: [
        {path: 'projects', component: ProjectsComponent},
        {path: 'tasks', component: TasksComponent},
        {path: 'notifications', component: NotificationsComponent}
    ]},
    {path: '**', component: NotFoundComponent}
];
