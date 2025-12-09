import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnrollmentService } from '../../../core/services/enrollment.service';
import { CareerService } from '../../../core/services/career.service';
import { Career, EnrollmentRequest, EnrollmentResponse } from '../../../core/models/models';

@Component({
  selector: 'app-enrollment-manage',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h2>📝 Gestión de Matrículas</h2>
      </div>

      @if (error) {<div class="alert alert-error">{{ error }}</div>}
      @if (success) {<div class="alert alert-success">{{ success }}</div>}

      <div class="grid-2">
        <!-- Matricular Estudiante -->
        <div class="card" style="background: #f9fafb;">
          <h3 style="color: #667eea; margin-bottom: 1rem;">✅ Matricular Estudiante</h3>
          <form (ngSubmit)="enrollStudent()">
            <div class="form-group">
              <label>ID del Estudiante *</label>
              <input type="number" class="form-control" [(ngModel)]="enrollData.studentId" 
                     name="enrollStudentId" min="1" required>
            </div>
            <div class="form-group">
              <label>Carrera *</label>
              <select class="form-control" [(ngModel)]="enrollData.careerId" name="enrollCareerId" required>
                <option value="0">Seleccionar carrera...</option>
                @for (career of careers; track career.id) {
                  <option [value]="career.id">{{ career.name }}</option>
                }
              </select>
            </div>
            <button type="submit" class="btn btn-success" [disabled]="enrolling" style="width: 100%;">
              {{ enrolling ? 'Matriculando...' : 'Matricular' }}
            </button>
          </form>
        </div>

        <!-- Desmatricular Estudiante -->
        <div class="card" style="background: #fef3c7;">
          <h3 style="color: #f59e0b; margin-bottom: 1rem;">❌ Desmatricular Estudiante</h3>
          <form (ngSubmit)="unenrollStudent()">
            <div class="form-group">
              <label>ID del Estudiante *</label>
              <input type="number" class="form-control" [(ngModel)]="unenrollData.studentId" 
                     name="unenrollStudentId" min="1" required>
            </div>
            <div class="form-group">
              <label>Carrera *</label>
              <select class="form-control" [(ngModel)]="unenrollData.careerId" name="unenrollCareerId" required>
                <option value="0">Seleccionar carrera...</option>
                @for (career of careers; track career.id) {
                  <option [value]="career.id">{{ career.name }}</option>
                }
              </select>
            </div>
            <button type="submit" class="btn btn-warning" [disabled]="unenrolling" style="width: 100%;">
              {{ unenrolling ? 'Desmatriculando...' : 'Desmatricular' }}
            </button>
          </form>
        </div>
      </div>

      <!-- Ver Matrículas de Estudiante -->
      <div class="card" style="margin-top: 2rem;">
        <h3 style="color: #667eea; margin-bottom: 1rem;">📊 Consultar Matrículas de Estudiante</h3>
        <div style="display: flex; gap: 1rem; align-items: flex-end;">
          <div class="form-group" style="flex: 1; margin-bottom: 0;">
            <label>ID del Estudiante</label>
            <input type="number" class="form-control" [(ngModel)]="searchStudentId" min="1">
          </div>
          <button class="btn btn-primary" (click)="loadStudentEnrollments()" [disabled]="loadingEnrollments">
            {{ loadingEnrollments ? 'Buscando...' : 'Buscar' }}
          </button>
        </div>

        @if (loadingEnrollments) {
          <div class="loading">Cargando matrículas</div>
        } @else if (studentEnrollments.length > 0) {
          <div class="table-container" style="margin-top: 1rem;">
            <table>
              <thead>
                <tr>
                  <th>ID Matrícula</th>
                  <th>ID Carrera</th>
                  <th>Fecha</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                @for (enrollment of studentEnrollments; track enrollment.enrollmentId) {
                  <tr>
                    <td>{{ enrollment.enrollmentId }}</td>
                    <td>{{ enrollment.careerId }}</td>
                    <td>{{ enrollment.enrollmentDate | date:'dd/MM/yyyy' }}</td>
                    <td>
                      <span [class]="enrollment.status === 'Success' ? 'badge badge-success' : 'badge badge-info'">
                        {{ enrollment.status }}
                      </span>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        } @else if (searchStudentId) {
          <div class="empty-state"><p>No se encontraron matrículas para este estudiante.</p></div>
        }
      </div>
    </div>
  `
})
export class EnrollmentManageComponent implements OnInit {
  careers: Career[] = [];
  studentEnrollments: EnrollmentResponse[] = [];
  
  enrollData: EnrollmentRequest = { studentId: 0, careerId: 0 };
  unenrollData: EnrollmentRequest = { studentId: 0, careerId: 0 };
  searchStudentId = 0;

  error = '';
  success = '';
  enrolling = false;
  unenrolling = false;
  loadingEnrollments = false;

  constructor(
    private enrollmentService: EnrollmentService,
    private careerService: CareerService
  ) {}

  ngOnInit(): void {
    this.loadCareers();
  }

  loadCareers(): void {
    this.careerService.getCareers().subscribe({
      next: (data: Career[]) => this.careers = data,
      error: () => this.error = 'Error al cargar carreras'
    });
  }

  enrollStudent(): void {
    if (!this.enrollData.studentId || !this.enrollData.careerId) {
      this.error = 'Por favor completa todos los campos';
      return;
    }

    this.enrolling = true;
    this.error = '';
    this.success = '';

    this.enrollmentService.enrollStudent(this.enrollData).subscribe({
      next: (response: EnrollmentResponse) => {
        if (response.status === 'Success') {
          this.success = `Estudiante matriculado exitosamente. ID Matrícula: ${response.enrollmentId}`;
          this.enrollData = { studentId: 0, careerId: 0 };
        } else {
          this.error = response.message || 'Error al matricular';
        }
        this.enrolling = false;
        setTimeout(() => this.success = '', 5000);
      },
      error: () => {
        this.error = 'Error al matricular estudiante';
        this.enrolling = false;
      }
    });
  }

  unenrollStudent(): void {
    if (!this.unenrollData.studentId || !this.unenrollData.careerId) {
      this.error = 'Por favor completa todos los campos';
      return;
    }

    this.unenrolling = true;
    this.error = '';
    this.success = '';

    this.enrollmentService.unenrollStudent(this.unenrollData).subscribe({
      next: (response: EnrollmentResponse) => {
        if (response.status === 'Success') {
          this.success = `Estudiante desmatriculado exitosamente`;
          this.unenrollData = { studentId: 0, careerId: 0 };
        } else {
          this.error = response.message || 'Error al desmatricular';
        }
        this.unenrolling = false;
        setTimeout(() => this.success = '', 5000);
      },
      error: () => {
        this.error = 'Error al desmatricular estudiante';
        this.unenrolling = false;
      }
    });
  }

  loadStudentEnrollments(): void {
    if (!this.searchStudentId || this.searchStudentId <= 0) {
      this.error = 'Por favor ingresa un ID de estudiante válido';
      return;
    }

    this.loadingEnrollments = true;
    this.error = '';

    this.enrollmentService.getStudentEnrollments(this.searchStudentId).subscribe({
      next: (data: EnrollmentResponse[]) => {
        this.studentEnrollments = data;
        this.loadingEnrollments = false;
      },
      error: () => {
        this.error = 'Error al cargar matrículas del estudiante';
        this.studentEnrollments = [];
        this.loadingEnrollments = false;
      }
    });
  }
}
