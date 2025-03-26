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
    const email = this.getEmailFromUrl();
    const name = this.getNameFromUrl();
    const picture = this.getPicturesFromUrl();

    if (token) {
      localStorage.setItem('jwt', token);

    if (email) {
      localStorage.setItem('email', email);
    } else {
      console.error('No email found in URL');
    }

    if (name) {
      localStorage.setItem('name', name);
    } else {
      console.error('No name found in URL');
    }

    if (picture) {
      localStorage.setItem('picture', picture);
    } else {
      console.error('No picture found in URL');
    }

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

  getEmailFromUrl(): string | null {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('email');
  }

  getNameFromUrl(): string | null {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('name');
  }

  getPicturesFromUrl(): string | null {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('picture');
  }
}
