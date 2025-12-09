import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-container">
      @if (authService.isAuthenticated()) {
        <nav class="navbar">
          <div class="navbar-content">
            <h1>🎓 Sistema Universidad</h1>
            <ul class="nav-links">
              <li><a routerLink="/students" routerLinkActive="active">Estudiantes</a></li>
              <li><a routerLink="/professors" routerLinkActive="active">Profesores</a></li>
              <li><a routerLink="/faculties" routerLinkActive="active">Facultades</a></li>
              <li><a routerLink="/careers" routerLinkActive="active">Carreras</a></li>
              <li><a routerLink="/enrollments" routerLinkActive="active">Matrículas</a></li>
              <li><a (click)="logout()" style="cursor: pointer;">Salir</a></li>
            </ul>
          </div>
        </nav>
      }
      
      <div class="container">
        <router-outlet></router-outlet>
      </div>
    </div>
  `
})
export class AppComponent {
  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
