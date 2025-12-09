import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FacultyService } from '../../../core/services/faculty.service';
import { Faculty } from '../../../core/models/models';

@Component({
  selector: 'app-faculty-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h2>🏛️ Gestión de Facultades</h2>
        <button class="btn btn-primary" (click)="openModal()">+ Nueva Facultad</button>
      </div>

      @if (error) {<div class="alert alert-error">{{ error }}</div>}
      @if (success) {<div class="alert alert-success">{{ success }}</div>}

      <div class="search-bar">
        <input type="text" [(ngModel)]="searchTerm" placeholder="Buscar facultades..." (keyup.enter)="loadFaculties()">
        <button class="btn btn-secondary btn-sm" style="margin-top: 0.5rem;" (click)="loadFaculties()">Buscar</button>
      </div>

      @if (loading) {
        <div class="loading">Cargando facultades</div>
      } @else if (faculties.length > 0) {
        <div class="table-container">
          <table>
            <thead><tr><th>ID</th><th>Nombre</th><th>Decano</th><th>Email</th><th>Teléfono</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>
              @for (faculty of faculties; track faculty.id) {
                <tr>
                  <td>{{ faculty.id }}</td>
                  <td>{{ faculty.name }}</td>
                  <td>{{ faculty.dean }}</td>
                  <td>{{ faculty.email }}</td>
                  <td>{{ faculty.phoneNumber }}</td>
                  <td><span [class]="faculty.isActive ? 'badge badge-success' : 'badge badge-danger'">
                    {{ faculty.isActive ? 'Activa' : 'Inactiva' }}</span></td>
                  <td>
                    <div class="table-actions">
                      <button class="btn btn-warning btn-sm" (click)="editFaculty(faculty)">Editar</button>
                      <button class="btn btn-danger btn-sm" (click)="deleteFaculty(faculty.id)">Eliminar</button>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="empty-state"><p>No hay facultades para mostrar.</p></div>
      }
    </div>

    @if (showModal) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ editMode ? 'Editar' : 'Nueva' }} Facultad</h3>
            <button class="btn btn-secondary btn-sm" (click)="closeModal()">✕</button>
          </div>
          <form (ngSubmit)="saveFaculty()">
            <div class="form-group"><label>Nombre *</label>
              <input type="text" class="form-control" [(ngModel)]="formData.name" name="name" required></div>
            <div class="form-group"><label>Descripción *</label>
              <textarea class="form-control" [(ngModel)]="formData.description" name="description" rows="3" required></textarea></div>
            <div class="form-group"><label>Decano *</label>
              <input type="text" class="form-control" [(ngModel)]="formData.dean" name="dean" required></div>
            <div class="grid-2">
              <div class="form-group"><label>Email *</label>
                <input type="email" class="form-control" [(ngModel)]="formData.email" name="email" required></div>
              <div class="form-group"><label>Teléfono *</label>
                <input type="tel" class="form-control" [(ngModel)]="formData.phoneNumber" name="phoneNumber" required></div>
            </div>
            <div class="form-group"><label>Fecha de Fundación *</label>
              <input type="date" class="form-control" [(ngModel)]="formData.foundedDate" name="foundedDate" required></div>
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
export class FacultyListComponent implements OnInit {
  faculties: Faculty[] = [];
  loading = false; error = ''; success = ''; showModal = false; editMode = false; saving = false; searchTerm = '';
  formData: Partial<Faculty> = {};

  constructor(private facultyService: FacultyService) {}

  ngOnInit(): void { this.loadFaculties(); }

  loadFaculties(): void {
    this.loading = true;
    this.facultyService.getFaculties(this.searchTerm).subscribe({
      next: (data: Faculty[]) => { this.faculties = data; this.loading = false; },
      error: () => { this.error = 'Error al cargar facultades'; this.loading = false; }
    });
  }

  openModal(): void {
    this.showModal = true; this.editMode = false;
    this.formData = { name: '', description: '', dean: '', email: '', phoneNumber: '', 
      foundedDate: new Date().toISOString(), isActive: true };
  }

  editFaculty(faculty: Faculty): void { this.showModal = true; this.editMode = true; this.formData = { ...faculty }; }
  closeModal(): void { this.showModal = false; }

  saveFaculty(): void {
    this.saving = true;
    const op = this.editMode && this.formData.id
      ? this.facultyService.updateFaculty(this.formData.id, this.formData)
      : this.facultyService.createFaculty(this.formData);
    op.subscribe({
      next: () => { this.success = `Facultad ${this.editMode ? 'actualizada' : 'creada'}`; this.closeModal(); 
        this.loadFaculties(); this.saving = false; setTimeout(() => this.success = '', 3000); },
      error: () => { this.error = 'Error al guardar'; this.saving = false; }
    });
  }

  deleteFaculty(id: number): void {
    if (!confirm('¿Eliminar esta facultad?')) return;
    this.facultyService.deleteFaculty(id).subscribe({
      next: () => { this.success = 'Facultad eliminada'; this.loadFaculties(); setTimeout(() => this.success = '', 3000); },
      error: () => this.error = 'Error al eliminar'
    });
  }
}
