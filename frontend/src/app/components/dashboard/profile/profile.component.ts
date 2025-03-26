import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {

  userName: string = localStorage.getItem('name') ?? '';
  email: string = localStorage.getItem('email') ?? '';
  userImage: string = localStorage.getItem('picture') ?? '';


  constructor(private router: Router) { }

  logout() {
    localStorage.removeItem('jwt');
    localStorage.removeItem('email');
    localStorage.removeItem('name');
    localStorage.removeItem('picture');
    this.router.navigate(['/login']);
   }
}
