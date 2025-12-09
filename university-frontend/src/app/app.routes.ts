import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'students',
    loadComponent: () => import('./features/students/student-list/student-list.component').then(m => m.StudentListComponent)
  },
  {
    path: 'professors',
    loadComponent: () => import('./features/professors/professor-list/professor-list.component').then(m => m.ProfessorListComponent)
  },
  {
    path: 'faculties',
    loadComponent: () => import('./features/faculties/faculty-list/faculty-list.component').then(m => m.FacultyListComponent)
  },
  {
    path: 'careers',
    loadComponent: () => import('./features/careers/career-list/career-list.component').then(m => m.CareerListComponent)
  },
  {
    path: 'enrollments',
    loadComponent: () => import('./features/enrollments/enrollment-manage/enrollment-manage.component').then(m => m.EnrollmentManageComponent)
  }
];
