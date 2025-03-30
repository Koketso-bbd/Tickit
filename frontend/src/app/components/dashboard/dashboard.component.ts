import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  userName: string = localStorage.getItem('name') ?? '';
  userImage: string = localStorage.getItem('picture') ?? '';
  
  constructor(private router: Router) { }

  logout() {
    localStorage.removeItem('jwt');
    localStorage.removeItem('email');
    localStorage.removeItem('name');
    localStorage.removeItem('picture');
    this.router.navigate(['/']);
   }
}
