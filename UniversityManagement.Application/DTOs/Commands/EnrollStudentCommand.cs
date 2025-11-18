using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Application.DTOs.Commands;

/// <summary>
/// Comando para matricular un estudiante en una carrera
/// </summary>
public class EnrollStudentCommand
{
    /// <summary>
    /// ID del estudiante a matricular
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del estudiante debe ser mayor a 0")]
    public int StudentId { get; set; }

    /// <summary>
    /// ID de la carrera en la que se matriculará
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de la carrera debe ser mayor a 0")]
    public int CareerId { get; set; }

    /// <summary>
    /// Fecha de matrícula (opcional, por defecto será la fecha actual)
    /// </summary>
    public DateTime? EnrollmentDate { get; set; }

    // Propiedades para auditoría y contexto (se llenan desde el controlador)
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}