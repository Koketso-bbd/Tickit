import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProjectsComponent } from './components/dashboard/projects/projects.component';
import { TasksComponent } from './components/dashboard/tasks/tasks.component';
import { NotificationsComponent } from './components/dashboard/notifications/notifications.component';
import { LoginComponent } from './components/login/login.component';
import { GoogleCallbackComponent } from './components/google-callback/google-callback.component';
import { ProjectDetailComponent } from './components/dashboard/project-detail/project-detail.component';
import { ProfileComponent } from './components/dashboard/profile/profile.component';
import { TasksFilterComponent } from './components/dashboard/task-filter/task-filter.component';

export const routes: Routes = [
    {path: '', component: LandingComponent},
    {path: 'dashboard', component: DashboardComponent, children: [
        {path: 'profile', component: ProfileComponent},
        {path: 'projects', component: ProjectsComponent},
        {path: 'projects/:id', component: ProjectDetailComponent},
        {path:'task-filter', component: TasksFilterComponent},
        {path: 'tasks', component: TasksComponent},
        {path: 'notifications', component: NotificationsComponent}
    ]},
    {path: 'login', component: LoginComponent},
    {path: 'google-callback', component: GoogleCallbackComponent},
    {path: '**', component: NotFoundComponent}
];
