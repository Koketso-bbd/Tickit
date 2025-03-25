import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-google-callback',
  imports: [],
  templateUrl: './google-callback.component.html',
  styleUrl: './google-callback.component.css'
})
export class GoogleCallbackComponent {
  constructor(private router: Router) { }

  ngOnInit(): void {
    const token = this.getTokenFromUrl();

    if (token) {
      localStorage.setItem('jwt', token);

      this.router.navigate(['/dashboard']);
    } else {
      console.error('No token found in URL');
      this.router.navigate(['/login']);
    }
  }

  getTokenFromUrl(): string | null {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('token');
  }
}
