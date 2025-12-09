import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="display: flex; justify-content: center; align-items: center; min-height: 100vh; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">
      <div class="card" style="max-width: 400px; width: 90%;">
        <div style="text-align: center; margin-bottom: 2rem;">
          <h1 style="color: #667eea; font-size: 2rem; margin-bottom: 0.5rem;">🎓 Sistema Universidad</h1>
          <p style="color: #6b7280;">Ingresa tus credenciales</p>
        </div>

        @if (error) {
          <div class="alert alert-error">
            {{ error }}
          </div>
        }

        <form (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Usuario</label>
            <input 
              type="text" 
              class="form-control" 
              [(ngModel)]="credentials.username" 
              name="username"
              placeholder="admin"
              required>
          </div>

          <div class="form-group">
            <label>Contraseña</label>
            <input 
              type="password" 
              class="form-control" 
              [(ngModel)]="credentials.password" 
              name="password"
              placeholder="••••••••"
              required>
          </div>

          <button type="submit" class="btn btn-primary" style="width: 100%;" [disabled]="loading">
            {{ loading ? 'Ingresando...' : 'Ingresar' }}
          </button>
        </form>

        <div style="margin-top: 1.5rem; padding-top: 1.5rem; border-top: 1px solid #e5e7eb; font-size: 0.875rem; color: #6b7280;">
          <p><strong>Usuarios de prueba:</strong></p>
          <p>Admin: admin / Admin123!</p>
          <p>Staff: staff / Staff123!</p>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  credentials = {
    username: '',
    password: ''
  };
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        this.authService.saveToken(response.token);
        this.router.navigate(['/students']);
      },
      error: (err) => {
        this.error = 'Credenciales inválidas. Por favor, intenta nuevamente.';
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}
