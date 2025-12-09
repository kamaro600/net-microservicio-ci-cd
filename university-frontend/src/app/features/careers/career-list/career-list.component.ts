import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CareerService } from '../../../core/services/career.service';
import { FacultyService } from '../../../core/services/faculty.service';
import { Career, Faculty } from '../../../core/models/models';

@Component({
  selector: 'app-career-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h2>🎓 Gestión de Carreras</h2>
        <button class="btn btn-primary" (click)="openModal()">+ Nueva Carrera</button>
      </div>

      @if (error) {<div class="alert alert-error">{{ error }}</div>}
      @if (success) {<div class="alert alert-success">{{ success }}</div>}

      <div class="search-bar">
        <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar carreras..." (keyup.enter)="loadCareers()">
        <button class="btn btn-secondary btn-sm" style="margin-top: 0.5rem;" (click)="loadCareers()">Buscar</button>
      </div>

      @if (loading) {
        <div class="loading">Cargando carreras</div>
      } @else if (careers.length > 0) {
        <div class="table-container">
          <table>
            <thead><tr><th>ID</th><th>Nombre</th><th>Facultad</th><th>Duración (años)</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>
              @for (career of careers; track career.id) {
                <tr>
                  <td>{{ career.id }}</td>
                  <td>{{ career.name }}</td>
                  <td>{{ career.facultyName || 'N/A' }}</td>
                  <td>{{ career.durationYears }}</td>
                  <td><span [class]="career.isActive ? 'badge badge-success' : 'badge badge-danger'">
                    {{ career.isActive ? 'Activa' : 'Inactiva' }}</span></td>
                  <td>
                    <div class="table-actions">
                      <button class="btn btn-warning btn-sm" (click)="editCareer(career)">Editar</button>
                      <button class="btn btn-danger btn-sm" (click)="deleteCareer(career.id)">Eliminar</button>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="empty-state"><p>No hay carreras para mostrar.</p></div>
      }
    </div>

    @if (showModal) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ editMode ? 'Editar' : 'Nueva' }} Carrera</h3>
            <button class="btn btn-secondary btn-sm" (click)="closeModal()">✕</button>
          </div>
          <form (ngSubmit)="saveCareer()">
            <div class="form-group"><label>Nombre *</label>
              <input type="text" class="form-control" [(ngModel)]="formData.name" name="name" required></div>
            <div class="form-group"><label>Descripción *</label>
              <textarea class="form-control" [(ngModel)]="formData.description" name="description" rows="3" required></textarea></div>
            <div class="grid-2">
              <div class="form-group"><label>Duración (años) *</label>
                <input type="number" class="form-control" [(ngModel)]="formData.durationYears" name="durationYears" min="1" max="10" required></div>
              <div class="form-group"><label>Facultad *</label>
                <select class="form-control" [(ngModel)]="formData.facultyId" name="facultyId" required>
                  <option value="">Seleccionar...</option>
                  @for (faculty of faculties; track faculty.id) {
                    <option [value]="faculty.id">{{ faculty.name }}</option>
                  }
                </select>
              </div>
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
export class CareerListComponent implements OnInit {
  careers: Career[] = [];
  faculties: Faculty[] = [];
  loading = false; error = ''; success = ''; showModal = false; editMode = false; saving = false; searchTerm = '';
  formData: Partial<Career> = {};

  constructor(
    private careerService: CareerService,
    private facultyService: FacultyService
  ) {}

  ngOnInit(): void { 
    this.loadCareers(); 
    this.loadFaculties();
  }

  loadCareers(): void {
    this.loading = true;
    this.careerService.getCareers(this.searchTerm).subscribe({
      next: (data: Career[]) => { this.careers = data; this.loading = false; },
      error: () => { this.error = 'Error al cargar carreras'; this.loading = false; }
    });
  }

  loadFaculties(): void {
    this.facultyService.getFaculties().subscribe({
      next: (data: Faculty[]) => this.faculties = data,
      error: () => this.error = 'Error al cargar facultades'
    });
  }

  openModal(): void {
    this.showModal = true; this.editMode = false;
    this.formData = { name: '', description: '', durationYears: 5, facultyId: 0, isActive: true };
  }

  editCareer(career: Career): void { this.showModal = true; this.editMode = true; this.formData = { ...career }; }
  closeModal(): void { this.showModal = false; }

  saveCareer(): void {
    this.saving = true;
    const op = this.editMode && this.formData.id
      ? this.careerService.updateCareer(this.formData.id, this.formData)
      : this.careerService.createCareer(this.formData);
    op.subscribe({
      next: () => { this.success = `Carrera ${this.editMode ? 'actualizada' : 'creada'}`; this.closeModal(); 
        this.loadCareers(); this.saving = false; setTimeout(() => this.success = '', 3000); },
      error: () => { this.error = 'Error al guardar'; this.saving = false; }
    });
  }

  deleteCareer(id: number): void {
    if (!confirm('¿Eliminar esta carrera?')) return;
    this.careerService.deleteCareer(id).subscribe({
      next: () => { this.success = 'Carrera eliminada'; this.loadCareers(); setTimeout(() => this.success = '', 3000); },
      error: () => this.error = 'Error al eliminar'
    });
  }
}
