import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Professor } from '../models/models';
import { environment } from '../config/environment';

@Injectable({
  providedIn: 'root'
})
export class ProfessorService {
  private apiUrl = `${environment.apiUrl}/professors`;

  constructor(private http: HttpClient) {}

  getProfessor(id: number): Observable<Professor> {
    return this.http.get<Professor>(`${this.apiUrl}/${id}`);
  }

  getProfessors(searchTerm?: string): Observable<Professor[]> {
    const url = searchTerm ? `${this.apiUrl}?searchTerm=${searchTerm}` : this.apiUrl;
    return this.http.get<Professor[]>(url);
  }

  createProfessor(professor: Partial<Professor>): Observable<Professor> {
    return this.http.post<Professor>(this.apiUrl, professor);
  }

  updateProfessor(id: number, professor: Partial<Professor>): Observable<Professor> {
    return this.http.put<Professor>(`${this.apiUrl}/${id}`, professor);
  }

  deleteProfessor(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
