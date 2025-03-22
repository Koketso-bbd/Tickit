import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';
import { NotFoundComponent } from './not-found/not-found.component';

export const routes: Routes = [
    {path: '', component: LandingComponent},
    {path: '**', component: NotFoundComponent}
];
