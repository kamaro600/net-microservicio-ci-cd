import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfessorService } from '../../../core/services/professor.service';
import { Professor } from '../../../core/models/models';

@Component({
  selector: 'app-professor-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h2>👨‍🏫 Gestión de Profesores</h2>
        <button class="btn btn-primary" (click)="openModal()">+ Nuevo Profesor</button>
      </div>

      @if (error) {
        <div class="alert alert-error">{{ error }}</div>
      }
      @if (success) {
        <div class="alert alert-success">{{ success }}</div>
      }

      <div class="search-bar">
        <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar profesores..."
               (keyup.enter)="loadProfessors()">
        <button class="btn btn-secondary btn-sm" style="margin-top: 0.5rem;" (click)="loadProfessors()">Buscar</button>
      </div>

      @if (loading) {
        <div class="loading">Cargando profesores</div>
      } @else if (professors.length > 0) {
        <div class="table-container">
          <table>
            <thead>
              <tr>
                <th>ID</th><th>Nombre</th><th>Email</th><th>Especialidad</th><th>Teléfono</th><th>Estado</th><th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              @for (prof of professors; track prof.id) {
                <tr>
                  <td>{{ prof.id }}</td>
                  <td>{{ prof.firstName }} {{ prof.lastName }}</td>
                  <td>{{ prof.email }}</td>
                  <td>{{ prof.specialty }}</td>
                  <td>{{ prof.phoneNumber }}</td>
                  <td><span [class]="prof.isActive ? 'badge badge-success' : 'badge badge-danger'">
                    {{ prof.isActive ? 'Activo' : 'Inactivo' }}</span></td>
                  <td>
                    <div class="table-actions">
                      <button class="btn btn-warning btn-sm" (click)="editProfessor(prof)">Editar</button>
                      <button class="btn btn-danger btn-sm" (click)="deleteProfessor(prof.id)">Eliminar</button>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="empty-state"><p>No hay profesores para mostrar.</p></div>
      }
    </div>

    @if (showModal) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ editMode ? 'Editar' : 'Nuevo' }} Profesor</h3>
            <button class="btn btn-secondary btn-sm" (click)="closeModal()">✕</button>
          </div>
          <form (ngSubmit)="saveProfessor()">
            <div class="grid-2">
              <div class="form-group">
                <label>Nombre *</label>
                <input type="text" class="form-control" [(ngModel)]="formData.firstName" name="firstName" required>
              </div>
              <div class="form-group">
                <label>Apellido *</label>
                <input type="text" class="form-control" [(ngModel)]="formData.lastName" name="lastName" required>
              </div>
            </div>
            <div class="form-group">
              <label>Email *</label>
              <input type="email" class="form-control" [(ngModel)]="formData.email" name="email" required>
            </div>
            <div class="grid-2">
              <div class="form-group">
                <label>Teléfono *</label>
                <input type="tel" class="form-control" [(ngModel)]="formData.phoneNumber" name="phoneNumber" required>
              </div>
              <div class="form-group">
                <label>Especialidad *</label>
                <input type="text" class="form-control" [(ngModel)]="formData.specialty" name="specialty" required>
              </div>
            </div>
            <div class="form-group">
              <label>Fecha de Contratación *</label>
              <input type="date" class="form-control" [(ngModel)]="formData.hireDate" name="hireDate" required>
            </div>
            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" (click)="closeModal()">Cancelar</button>
              <button type="submit" class="btn btn-primary" [disabled]="saving">{{ saving ? 'Guardando...' : 'Guardar' }}</button>
            </div>
          </form>
        </div>
      </div>
    }
  `
})
export class ProfessorListComponent implements OnInit {
  professors: Professor[] = [];
  loading = false;
  error = '';
  success = '';
  showModal = false;
  editMode = false;
  saving = false;
  searchTerm = '';
  formData: Partial<Professor> = {};

  constructor(private professorService: ProfessorService) {}

  ngOnInit(): void {
    this.loadProfessors();
  }

  loadProfessors(): void {
    this.loading = true;
    this.professorService.getProfessors(this.searchTerm).subscribe({
      next: (data: Professor[]) => {
        this.professors = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Error al cargar profesores';
        this.loading = false;
      }
    });
  }

  openModal(): void {
    this.showModal = true;
    this.editMode = false;
    this.formData = {
      firstName: '', lastName: '', email: '', phoneNumber: '', 
      specialty: '', hireDate: new Date().toISOString(), isActive: true
    };
  }

  editProfessor(prof: Professor): void {
    this.showModal = true;
    this.editMode = true;
    this.formData = { ...prof };
  }

  closeModal(): void {
    this.showModal = false;
  }

  saveProfessor(): void {
    this.saving = true;
    const operation = this.editMode && this.formData.id
      ? this.professorService.updateProfessor(this.formData.id, this.formData)
      : this.professorService.createProfessor(this.formData);

    operation.subscribe({
      next: () => {
        this.success = `Profesor ${this.editMode ? 'actualizado' : 'creado'} exitosamente`;
        this.closeModal();
        this.loadProfessors();
        this.saving = false;
        setTimeout(() => this.success = '', 3000);
      },
      error: () => {
        this.error = 'Error al guardar profesor';
        this.saving = false;
      }
    });
  }

  deleteProfessor(id: number): void {
    if (!confirm('¿Eliminar este profesor?')) return;
    this.professorService.deleteProfessor(id).subscribe({
      next: () => {
        this.success = 'Profesor eliminado';
        this.loadProfessors();
        setTimeout(() => this.success = '', 3000);
      },
      error: () => this.error = 'Error al eliminar'
    });
  }
}
