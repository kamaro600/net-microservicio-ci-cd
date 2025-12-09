import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Faculty } from '../models/models';
import { environment } from '../config/environment';

@Injectable({
  providedIn: 'root'
})
export class FacultyService {
  private apiUrl = `${environment.apiUrl}/faculties`;

  constructor(private http: HttpClient) {}

  getFaculty(id: number): Observable<Faculty> {
    return this.http.get<Faculty>(`${this.apiUrl}/${id}`);
  }

  getFaculties(searchTerm?: string): Observable<Faculty[]> {
    const url = searchTerm ? `${this.apiUrl}?searchTerm=${searchTerm}` : this.apiUrl;
    return this.http.get<Faculty[]>(url);
  }

  createFaculty(faculty: Partial<Faculty>): Observable<Faculty> {
    return this.http.post<Faculty>(this.apiUrl, faculty);
  }

  updateFaculty(id: number, faculty: Partial<Faculty>): Observable<Faculty> {
    return this.http.put<Faculty>(`${this.apiUrl}/${id}`, faculty);
  }

  deleteFaculty(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
