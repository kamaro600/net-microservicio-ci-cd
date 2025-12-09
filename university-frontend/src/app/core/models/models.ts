export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  address: string;
  enrollmentDate: string;
  isActive: boolean;
}

export interface Professor {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  specialty: string;
  hireDate: string;
  isActive: boolean;
}

export interface Faculty {
  id: number;
  name: string;
  description: string;
  foundedDate: string;
  dean: string;
  email: string;
  phoneNumber: string;
  isActive: boolean;
}

export interface Career {
  id: number;
  name: string;
  description: string;
  durationYears: number;
  facultyId: number;
  facultyName?: string;
  isActive: boolean;
}

export interface EnrollmentRequest {
  studentId: number;
  careerId: number;
}

export interface EnrollmentResponse {
  enrollmentId: number;
  studentId: number;
  careerId: number;
  enrollmentDate: string;
  status: string;
  message: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  expiresAt: string;
}
