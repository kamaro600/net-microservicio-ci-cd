import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Career } from '../models/models';
import { environment } from '../config/environment';

@Injectable({
  providedIn: 'root'
})
export class CareerService {
  private apiUrl = `${environment.apiUrl}/careers`;

  constructor(private http: HttpClient) {}

  getCareer(id: number): Observable<Career> {
    return this.http.get<Career>(`${this.apiUrl}/${id}`);
  }

  getCareers(searchTerm?: string): Observable<Career[]> {
    const url = searchTerm ? `${this.apiUrl}?searchTerm=${searchTerm}` : this.apiUrl;
    return this.http.get<Career[]>(url);
  }

  getCareersByFaculty(facultyId: number): Observable<Career[]> {
    return this.http.get<Career[]>(`${this.apiUrl}/faculty/${facultyId}`);
  }

  createCareer(career: Partial<Career>): Observable<Career> {
    return this.http.post<Career>(this.apiUrl, career);
  }

  updateCareer(id: number, career: Partial<Career>): Observable<Career> {
    return this.http.put<Career>(`${this.apiUrl}/${id}`, career);
  }

  deleteCareer(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
