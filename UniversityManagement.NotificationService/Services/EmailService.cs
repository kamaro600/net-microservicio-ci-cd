using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using UniversityManagement.NotificationService.Configuration;

namespace UniversityManagement.NotificationService.Services;

/// <summary>
/// Servicio para envío de emails
/// </summary>
public interface IEmailService
{
    Task SendEnrollmentConfirmationAsync(string email, string ownerName, string course, DateTime enrollmentDate);
    Task SendEnrollmentCancellationAsync(string email, string ownerName, string course, DateTime enrollmentDate);
    Task SendWelcomeAsync(string email, string fullName);
    Task SendStudentUpdateNotificationAsync(string email, string nombre, List<string> eventos);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<SmtpSettings> smtpSettings)
    {
        _logger = logger;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEnrollmentConfirmationAsync(string email, string ownerName, string course, DateTime enrollmentDate)
    {
        try
        {
            _logger.LogInformation("=== Iniciando envío de Email de matrícula ===");
            _logger.LogInformation("Destinatario: {email}", email);
            _logger.LogInformation("SMTP Host: {host}:{port}", _smtpSettings.Host, _smtpSettings.Port);
            _logger.LogInformation("SMTP User: {user}", _smtpSettings.Username);
            _logger.LogInformation("SSL Enabled: {ssl}", _smtpSettings.EnableSsl);
            _logger.LogInformation("From: {from}", _smtpSettings.FromEmail);

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            _logger.LogInformation("SmtpClient configurado, preparando mensaje...");

            var message = new MailMessage(_smtpSettings.FromEmail, email)
            {
                Subject = "Confirmación de Matrícula - Universidad",
                Body = $@"
                    <h2>¡Hola {ownerName}!</h2>
                    <p>Te confirmamos que tu matrícula para <strong>{course}</strong> se completó exitosamente el día {enrollmentDate:dd/MM/yyyy}.</p>
                    <p>¡Bienvenido a nuestra universidad! Esperamos que tengas una excelente experiencia académica.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>
                ",
                IsBodyHtml = true
            };

            message.From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
            
            _logger.LogInformation("Enviando mensaje vía SMTP...");
            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("✅ Email de confirmación de matrícula enviado exitosamente a {email}", email);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "❌ Error SMTP enviando Email a {email}. StatusCode: {status}", email, smtpEx.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error general enviando Email de confirmación de matrícula a {email}", email);
            throw;
        }
    }

    public async Task SendEnrollmentCancellationAsync(string email, string ownerName, string course, DateTime enrollmentDate)
    {
        try
        {
            _logger.LogInformation("Enviando Email de cancelación de matrícula a {email}: {Message}", email, ownerName);

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var message = new MailMessage(_smtpSettings.FromEmail, email)
            {
                Subject = "Cancelación de Matrícula - Universidad",
                Body = $@"
                    <h2>Hola {ownerName},</h2>
                    <p>Te confirmamos que tu cancelación de matrícula para <strong>{course}</strong> se completó el día {enrollmentDate:dd/MM/yyyy}.</p>
                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>
                ",
                IsBodyHtml = true
            };

            message.From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email de cancelación de matrícula enviado exitosamente a {email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando Email de cancelación de matrícula a {email}", email);
        }
    }

    public async Task SendWelcomeAsync(string email, string fullName)
    {
        try
        {
            _logger.LogInformation("Enviando Email de bienvenida a {email}: {fullName}", email, fullName);

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var message = new MailMessage(_smtpSettings.FromEmail, email)
            {
                Subject = "¡Bienvenido a la Universidad!",
                Body = $@"
                    <h2>¡Bienvenido {fullName}!</h2>
                    <p>Te damos la bienvenida a nuestra universidad. Te has registrado correctamente en nuestro sistema.</p>
                    <p>Esperamos que tengas una excelente experiencia académica con nosotros.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>
                ",
                IsBodyHtml = true
            };

            message.From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email de bienvenida enviado exitosamente a {email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando Email de bienvenida a {email}", email);
        }
    }

    public async Task SendStudentUpdateNotificationAsync(string email, string nombre, List<string> eventos)
    {
        try
        {
            _logger.LogInformation("Enviando Email de actualización a {email}: {nombre}", email, nombre);

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var eventosTexto = string.Join("", eventos.Select(e => $"<li>{e}</li>"));

            var message = new MailMessage(_smtpSettings.FromEmail, email)
            {
                Subject = "Actualización - Gestión Universitaria",
                Body = $@"
                    <h2>Hola {nombre},</h2>
                    <p>Te notificamos que han ocurrido los siguientes eventos en tu cuenta:</p>
                    <ul>
                        {eventosTexto}
                    </ul>
                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>
                ",
                IsBodyHtml = true
            };

            message.From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email de actualización enviado exitosamente a {email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando Email de actualización a {email}", email);
        }
    }
}