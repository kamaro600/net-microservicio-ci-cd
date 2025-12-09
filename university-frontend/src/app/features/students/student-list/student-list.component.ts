import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentService } from '../../../core/services/student.service';
import { Student } from '../../../core/models/models';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h2>📚 Gestión de Estudiantes</h2>
        <button class="btn btn-primary" (click)="openModal()">
          + Nuevo Estudiante
        </button>
      </div>

      @if (error) {
        <div class="alert alert-error">{{ error }}</div>
      }

      @if (success) {
        <div class="alert alert-success">{{ success }}</div>
      }

      <div class="search-bar">
        <input 
          type="text" 
          [(ngModel)]="searchId"
          placeholder="Buscar por ID de estudiante..." 
          (keyup.enter)="searchStudent()">
        <button class="btn btn-secondary btn-sm" style="margin-top: 0.5rem;" (click)="searchStudent()">
          Buscar
        </button>
      </div>

      @if (loading) {
        <div class="loading">Cargando estudiantes</div>
      } @else if (student) {
        <div class="table-container">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Email</th>
                <th>Teléfono</th>
                <th>Fecha Nac.</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>{{ student.id }}</td>
                <td>{{ student.firstName }} {{ student.lastName }}</td>
                <td>{{ student.email }}</td>
                <td>{{ student.phoneNumber }}</td>
                <td>{{ student.dateOfBirth | date:'dd/MM/yyyy' }}</td>
                <td>
                  <span [class]="student.isActive ? 'badge badge-success' : 'badge badge-danger'">
                    {{ student.isActive ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td>
                  <div class="table-actions">
                    <button class="btn btn-warning btn-sm" (click)="editStudent(student)">
                      Editar
                    </button>
                    <button class="btn btn-danger btn-sm" (click)="deleteStudent(student.id)">
                      Eliminar
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      } @else {
        <div class="empty-state">
          <p>No hay estudiantes para mostrar. Busca por ID o crea uno nuevo.</p>
        </div>
      }
    </div>

    @if (showModal) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ editMode ? 'Editar' : 'Nuevo' }} Estudiante</h3>
            <button class="btn btn-secondary btn-sm" (click)="closeModal()">✕</button>
          </div>

          <form (ngSubmit)="saveStudent()">
            <div class="grid-2">
              <div class="form-group">
                <label>Nombre *</label>
                <input 
                  type="text" 
                  class="form-control" 
                  [(ngModel)]="formData.firstName" 
                  name="firstName"
                  required>
              </div>

              <div class="form-group">
                <label>Apellido *</label>
                <input 
                  type="text" 
                  class="form-control" 
                  [(ngModel)]="formData.lastName" 
                  name="lastName"
                  required>
              </div>
            </div>

            <div class="form-group">
              <label>Email *</label>
              <input 
                type="email" 
                class="form-control" 
                [(ngModel)]="formData.email" 
                name="email"
                required>
            </div>

            <div class="grid-2">
              <div class="form-group">
                <label>Teléfono *</label>
                <input 
                  type="tel" 
                  class="form-control" 
                  [(ngModel)]="formData.phoneNumber" 
                  name="phoneNumber"
                  required>
              </div>

              <div class="form-group">
                <label>Fecha de Nacimiento *</label>
                <input 
                  type="date" 
                  class="form-control" 
                  [(ngModel)]="formData.dateOfBirth" 
                  name="dateOfBirth"
                  required>
              </div>
            </div>

            <div class="form-group">
              <label>Dirección *</label>
              <input 
                type="text" 
                class="form-control" 
                [(ngModel)]="formData.address" 
                name="address"
                required>
            </div>

            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" (click)="closeModal()">
                Cancelar
              </button>
              <button type="submit" class="btn btn-primary" [disabled]="saving">
                {{ saving ? 'Guardando...' : 'Guardar' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }
  `
})
export class StudentListComponent implements OnInit {
  student: Student | null = null;
  loading = false;
  error = '';
  success = '';
  showModal = false;
  editMode = false;
  saving = false;
  searchId = '';

  formData: Partial<Student> = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: '',
    address: '',
    enrollmentDate: new Date().toISOString(),
    isActive: true
  };

  constructor(private studentService: StudentService) {}

  ngOnInit(): void {
    // No cargamos todos los estudiantes automáticamente
  }

  searchStudent(): void {
    const id = parseInt(this.searchId);
    if (!id || id <= 0) {
      this.error = 'Por favor ingresa un ID válido';
      return;
    }

    this.loading = true;
    this.error = '';
    this.studentService.getStudent(id).subscribe({
      next: (data: Student) => {
        this.student = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'No se encontró el estudiante con ese ID';
        this.student = null;
        this.loading = false;
      }
    });
  }

  openModal(): void {
    this.showModal = true;
    this.editMode = false;
    this.formData = {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      dateOfBirth: '',
      address: '',
      enrollmentDate: new Date().toISOString(),
      isActive: true
    };
  }

  editStudent(student: Student): void {
    this.showModal = true;
    this.editMode = true;
    this.formData = { ...student };
  }

  closeModal(): void {
    this.showModal = false;
    this.formData = {};
  }

  saveStudent(): void {
    this.saving = true;
    this.error = '';

    const operation = this.editMode && this.formData.id
      ? this.studentService.updateStudent(this.formData.id, this.formData)
      : this.studentService.createStudent(this.formData);

    operation.subscribe({
      next: () => {
        this.success = `Estudiante ${this.editMode ? 'actualizado' : 'creado'} exitosamente`;
        this.closeModal();
        this.saving = false;
        if (this.formData.id) {
          this.searchId = this.formData.id.toString();
          this.searchStudent();
        }
        setTimeout(() => this.success = '', 3000);
      },
      error: () => {
        this.error = 'Error al guardar el estudiante';
        this.saving = false;
      }
    });
  }

  deleteStudent(id: number): void {
    if (!confirm('¿Estás seguro de eliminar este estudiante?')) return;

    this.studentService.deleteStudent(id).subscribe({
      next: () => {
        this.success = 'Estudiante eliminado exitosamente';
        this.student = null;
        setTimeout(() => this.success = '', 3000);
      },
      error: () => {
        this.error = 'Error al eliminar el estudiante';
      }
    });
  }
}
