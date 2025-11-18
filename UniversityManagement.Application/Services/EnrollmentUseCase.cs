using UniversityManagement.Application.DTOs.Commands;
using UniversityManagement.Application.DTOs.Responses;
using UniversityManagement.Application.Ports.In;
using UniversityManagement.Domain.Models;
using UniversityManagement.Domain.Repositories;
using UniversityManagement.Domain.Services.Interfaces;
using UniversityManagement.Domain.Events;
using UniversityManagement.Application.Events;

namespace UniversityManagement.Application.Services;

/// <summary>
/// Implementación de casos de uso para matrícula de estudiantes
/// </summary>
public class EnrollmentUseCase : IEnrollmentUseCase
{
    private readonly IStudentRepository _studentRepository;
    private readonly ICareerRepository _careerRepository;
    private readonly IStudentCareerRepository _studentCareerRepository;
    private readonly IStudentDomainService _studentDomainService;
    private readonly StudentEnrolledEventHandler _enrolledEventHandler;
    private readonly StudentUnenrolledEventHandler _unenrolledEventHandler;

    public EnrollmentUseCase(
        IStudentRepository studentRepository,
        ICareerRepository careerRepository,
        IStudentCareerRepository studentCareerRepository,
        IStudentDomainService studentDomainService,
        StudentEnrolledEventHandler enrolledEventHandler,
        StudentUnenrolledEventHandler unenrolledEventHandler)
    {
        _studentRepository = studentRepository;
        _careerRepository = careerRepository;
        _studentCareerRepository = studentCareerRepository;
        _studentDomainService = studentDomainService;
        _enrolledEventHandler = enrolledEventHandler;
        _unenrolledEventHandler = unenrolledEventHandler;
    }

    public async Task<EnrollmentResponse> EnrollStudentInCareerAsync(EnrollStudentCommand command)
    {
        try
        {
            // Verificar que el estudiante existe
            var student = await _studentRepository.GetByIdAsync(command.StudentId);
            if (student == null)
            {
                return new EnrollmentResponse
                {
                    Status = "Error",
                    Message = $"Estudiante con ID {command.StudentId} no encontrado"
                };
            }

            // Verificar que la carrera existe
            var career = await _careerRepository.GetByIdAsync(command.CareerId);
            if (career == null)
            {
                return new EnrollmentResponse
                {
                    Status = "Error",
                    Message = $"Carrera con ID {command.CareerId} no encontrada"
                };
            }

            // Verificar si ya existe una matrícula (activa o inactiva)
            var existingEnrollment = await _studentCareerRepository.GetEnrollmentAsync(command.StudentId, command.CareerId);
            
            if (existingEnrollment != null && existingEnrollment.IsActive)
            {
                return new EnrollmentResponse
                {
                    Status = "Error",
                    Message = "El estudiante ya está matriculado activamente en esta carrera"
                };
            }

            // Verificar si el estudiante puede matricularse (reglas de negocio)
            if (!student.CanEnrollInCareer(career))
            {
                return new EnrollmentResponse
                {
                    Status = "Error",
                    Message = "El estudiante no cumple los requisitos para matricularse en esta carrera"
                };
            }

            StudentCareer savedEnrollment;

            if (existingEnrollment != null && !existingEnrollment.IsActive)
            {
                // Reactivar matrícula existente
                existingEnrollment.Activate();
                savedEnrollment = await _studentCareerRepository.UpdateEnrollmentAsync(existingEnrollment);
            }
            else
            {
                // Crear nueva matrícula
                var enrollment = await _studentDomainService.EnrollStudentInCareerAsync(student, career);
                savedEnrollment = await _studentCareerRepository.AddEnrollmentAsync(enrollment);
            }

            // Publicar evento de dominio para notificación y auditoría
            var enrolledEvent = new StudentEnrolledEvent(
                studentId: savedEnrollment.StudentId,
                careerId: savedEnrollment.CareerId,
                studentName: savedEnrollment.Student?.FullName.FullDisplayName ?? "",
                careerName: savedEnrollment.Career?.Name ?? "",
                studentEmail: savedEnrollment.Student?.Email?.Value ?? "",
                enrollmentDate: savedEnrollment.EnrollmentDate,
                enrolledBy: command.UserId ?? "system"
            );

            // Procesar el evento usando el handler
            await _enrolledEventHandler.HandleAsync(enrolledEvent);

            return new EnrollmentResponse
            {
                StudentId = savedEnrollment.StudentId,
                StudentName = savedEnrollment.Student?.FullName.FullDisplayName ?? "",
                StudentDni = savedEnrollment.Student?.Dni.Value ?? "",
                CareerId = savedEnrollment.CareerId,
                CareerName = savedEnrollment.Career?.Name ?? "",
                FacultyName = "", // La carrera no tiene navegación directa a Faculty en el dominio
                EnrollmentDate = savedEnrollment.EnrollmentDate,
                IsActive = savedEnrollment.IsActive,
                Status = "Success",
                Message = "Estudiante matriculado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new EnrollmentResponse
            {
                Status = "Error",
                Message = $"Error al matricular estudiante: {ex.Message}"
            };
        }
    }

    public async Task<EnrollmentResponse> UnenrollStudentFromCareerAsync(UnenrollStudentCommand command)
    {
        try
        {
            // Verificar que existe la matrícula
            var enrollment = await _studentCareerRepository.GetEnrollmentAsync(command.StudentId, command.CareerId);
            if (enrollment == null)
            {
                return new EnrollmentResponse
                {
                    Status = "Error",
                    Message = "No se encontró la matrícula especificada"
                };
            }

            if (!enrollment.IsActive)
            {
                return new EnrollmentResponse
                {
                    Status = "Error",
                    Message = "La matrícula ya está inactiva"
                };
            }

            // Desmatricular usando método del dominio
            enrollment.Unenroll();

            // Actualizar en la base de datos
            var updatedEnrollment = await _studentCareerRepository.UpdateEnrollmentAsync(enrollment);

            // Publicar evento de dominio para notificación y auditoría
            var unenrolledEvent = new StudentUnenrolledEvent(
                studentId: updatedEnrollment.StudentId,
                careerId: updatedEnrollment.CareerId,
                studentName: updatedEnrollment.Student?.FullName.FullDisplayName ?? "",
                careerName: updatedEnrollment.Career?.Name ?? "",
                studentEmail: updatedEnrollment.Student?.Email?.Value ?? "",
                unenrollmentDate: DateTime.UtcNow,
                unenrolledBy: command.UserId ?? "system"
            );

            // Procesar el evento usando el handler
            await _unenrolledEventHandler.HandleAsync(unenrolledEvent);

            return new EnrollmentResponse
            {
                StudentId = updatedEnrollment.StudentId,
                StudentName = updatedEnrollment.Student?.FullName.FullDisplayName ?? "",
                StudentDni = updatedEnrollment.Student?.Dni.Value ?? "",
                CareerId = updatedEnrollment.CareerId,
                CareerName = updatedEnrollment.Career?.Name ?? "",
                FacultyName = "", // La carrera no tiene navegación directa a Faculty en el dominio
                EnrollmentDate = updatedEnrollment.EnrollmentDate,
                IsActive = updatedEnrollment.IsActive,
                Status = "Success",
                Message = "Estudiante desmatriculado exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new EnrollmentResponse
            {
                Status = "Error",
                Message = $"Error al desmatricular estudiante: {ex.Message}"
            };
        }
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetStudentEnrollmentsAsync(int studentId)
    {
        try
        {
            var enrollments = await _studentCareerRepository.GetStudentEnrollmentsAsync(studentId);

            return enrollments.Select(e => new EnrollmentResponse
            {
                StudentId = e.StudentId,
                StudentName = e.Student?.FullName.FullDisplayName ?? "",
                StudentDni = e.Student?.Dni.Value ?? "",
                CareerId = e.CareerId,
                CareerName = e.Career?.Name ?? "",
                FacultyName = "", // La carrera no tiene navegación directa a Faculty en el dominio
                EnrollmentDate = e.EnrollmentDate,
                IsActive = e.IsActive,
                Status = "Success",
                Message = ""
            });
        }
        catch (Exception ex)
        {
            return new List<EnrollmentResponse>
            {
                new EnrollmentResponse
                {
                    Status = "Error",
                    Message = $"Error al obtener matrículas del estudiante: {ex.Message}"
                }
            };
        }
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetCareerEnrollmentsAsync(int careerId)
    {
        try
        {
            var enrollments = await _studentCareerRepository.GetCareerEnrollmentsAsync(careerId);

            return enrollments.Select(e => new EnrollmentResponse
            {
                StudentId = e.StudentId,
                StudentName = e.Student?.FullName.FullDisplayName ?? "",
                StudentDni = e.Student?.Dni.Value ?? "",
                CareerId = e.CareerId,
                CareerName = e.Career?.Name ?? "",
                FacultyName = "", // La carrera no tiene navegación directa a Faculty en el dominio
                EnrollmentDate = e.EnrollmentDate,
                IsActive = e.IsActive,
                Status = "Success",
                Message = ""
            });
        }
        catch (Exception ex)
        {
            return new List<EnrollmentResponse>
            {
                new EnrollmentResponse
                {
                    Status = "Error",
                    Message = $"Error al obtener matrículas de la carrera: {ex.Message}"
                }
            };
        }
    }
}