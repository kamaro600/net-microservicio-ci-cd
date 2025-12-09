import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EnrollmentRequest, EnrollmentResponse } from '../models/models';
import { environment } from '../config/environment';

@Injectable({
  providedIn: 'root'
})
export class EnrollmentService {
  private apiUrl = `${environment.apiUrl}/enrollment`;

  constructor(private http: HttpClient) {}

  enrollStudent(enrollment: EnrollmentRequest): Observable<EnrollmentResponse> {
    return this.http.post<EnrollmentResponse>(`${this.apiUrl}/enroll`, enrollment);
  }

  unenrollStudent(enrollment: EnrollmentRequest): Observable<EnrollmentResponse> {
    return this.http.post<EnrollmentResponse>(`${this.apiUrl}/unenroll`, enrollment);
  }

  getStudentEnrollments(studentId: number): Observable<EnrollmentResponse[]> {
    return this.http.get<EnrollmentResponse[]>(`${this.apiUrl}/student/${studentId}`);
  }
}
