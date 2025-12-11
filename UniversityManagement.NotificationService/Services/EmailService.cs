using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
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
    private readonly HttpClient _httpClient;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<SmtpSettings> smtpSettings,
        HttpClient httpClient)
    {
        _logger = logger;
        _smtpSettings = smtpSettings.Value;
        _httpClient = httpClient;
    }

    public async Task SendEnrollmentConfirmationAsync(string email, string ownerName, string course, DateTime enrollmentDate)
    {
        try
        {
            _logger.LogInformation("=== Iniciando envío de Email de matrícula ===");
            _logger.LogInformation("Destinatario: {email}", email);
            _logger.LogInformation("Método: {method}", _smtpSettings.UseApi ? "Mailtrap API" : "SMTP");

            if (_smtpSettings.UseApi)
            {
                await SendViaMailtrapApiAsync(email, ownerName, course, enrollmentDate, isEnrollment: true);
            }
            else
            {
                await SendViaSmtpAsync(email, ownerName, course, enrollmentDate, isEnrollment: true);
            }

            _logger.LogInformation("✅ Email de confirmación de matrícula enviado exitosamente a {email}", email);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("❌ Timeout enviando Email a {email}. El servicio no respondió en 15 segundos. Posible bloqueo de puerto o credenciales incorrectas.", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error general enviando Email de confirmación de matrícula a {email}. Tipo: {type}", email, ex.GetType().Name);
        }
    }

    private async Task SendViaMailtrapApiAsync(string email, string ownerName, string course, DateTime enrollmentDate, bool isEnrollment)
    {
        _logger.LogInformation("API URL: {url}", _smtpSettings.ApiUrl);
        _logger.LogInformation("API Token: {token}", _smtpSettings.ApiToken?.Substring(0, Math.Min(10, _smtpSettings.ApiToken?.Length ?? 0)) + "...");

        var emailPayload = new
        {
            from = new { email = _smtpSettings.FromEmail, name = _smtpSettings.FromName },
            to = new[] { new { email = email } },
            subject = isEnrollment ? "Confirmación de Matrícula - Universidad" : "Cancelación de Matrícula - Universidad",
            html = isEnrollment 
                ? $@"<h2>¡Hola {ownerName}!</h2>
                    <p>Te confirmamos que tu matrícula para <strong>{course}</strong> se completó exitosamente el día {enrollmentDate:dd/MM/yyyy}.</p>
                    <p>¡Bienvenido a nuestra universidad! Esperamos que tengas una excelente experiencia académica.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>"
                : $@"<h2>Hola {ownerName},</h2>
                    <p>Te confirmamos que tu cancelación de matrícula para <strong>{course}</strong> se completó el día {enrollmentDate:dd/MM/yyyy}.</p>
                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>"
        };

        var json = JsonSerializer.Serialize(emailPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_smtpSettings.ApiToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _logger.LogInformation("Enviando Email vía Mailtrap API...");
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var response = await _httpClient.PostAsync(_smtpSettings.ApiUrl, content, cts.Token);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("❌ Error en Mailtrap API. Status: {status}, Response: {response}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Mailtrap API error: {response.StatusCode} - {responseBody}");
        }

        _logger.LogInformation("✅ Respuesta Mailtrap API: {response}", responseBody);
    }

    private async Task SendViaSmtpAsync(string email, string ownerName, string course, DateTime enrollmentDate, bool isEnrollment)
    {
        _logger.LogInformation("SMTP Host: {host}:{port}", _smtpSettings.Host, _smtpSettings.Port);
        _logger.LogInformation("SMTP User: {user}", _smtpSettings.Username);
        _logger.LogInformation("SSL Enabled: {ssl}", _smtpSettings.EnableSsl);
        _logger.LogInformation("From: {from}", _smtpSettings.FromEmail);

        using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
            EnableSsl = _smtpSettings.EnableSsl,
            Timeout = 10000, // 10 segundos timeout
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        _logger.LogInformation("SmtpClient configurado, preparando mensaje...");

        var message = new MailMessage(_smtpSettings.FromEmail, email)
        {
            Subject = isEnrollment ? "Confirmación de Matrícula - Universidad" : "Cancelación de Matrícula - Universidad",
            Body = isEnrollment 
                ? $@"<h2>¡Hola {ownerName}!</h2>
                    <p>Te confirmamos que tu matrícula para <strong>{course}</strong> se completó exitosamente el día {enrollmentDate:dd/MM/yyyy}.</p>
                    <p>¡Bienvenido a nuestra universidad! Esperamos que tengas una excelente experiencia académica.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>"
                : $@"<h2>Hola {ownerName},</h2>
                    <p>Te confirmamos que tu cancelación de matrícula para <strong>{course}</strong> se completó el día {enrollmentDate:dd/MM/yyyy}.</p>
                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    <br>
                    <p>Saludos,<br>{_smtpSettings.FromName}</p>",
            IsBodyHtml = true
        };

        message.From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
        
        _logger.LogInformation("Enviando mensaje vía SMTP...");
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await smtpClient.SendMailAsync(message, cts.Token);
    }

    public async Task SendEnrollmentCancellationAsync(string email, string ownerName, string course, DateTime enrollmentDate)
    {
        try
        {
            _logger.LogInformation("Enviando Email de cancelación de matrícula a {email}: {Message}", email, ownerName);

            if (_smtpSettings.UseApi)
            {
                await SendViaMailtrapApiAsync(email, ownerName, course, enrollmentDate, isEnrollment: false);
            }
            else
            {
                await SendViaSmtpAsync(email, ownerName, course, enrollmentDate, isEnrollment: false);
            }

            _logger.LogInformation("Email de cancelación de matrícula enviado exitosamente a {email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando Email de cancelación de matrícula a {email}", email);
            throw;
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