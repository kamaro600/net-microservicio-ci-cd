namespace UniversityManagement.NotificationService.Configuration;

/// <summary>
/// Configuración para RabbitMQ
/// </summary>
public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "university.notifications";
    public string EnrollmentQueueName { get; set; } = "enrollment.notifications";
    public string UnenrollmentQueueName { get; set; } = "unenrollment.notifications";
    public string EnrollmentRoutingKey { get; set; } = "enrollment.created";
    public string UnenrollmentRoutingKey { get; set; } = "enrollment.deleted";
}

/// <summary>
/// Configuración para SMTP
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@universidad.edu";
    public string FromName { get; set; } = "Universidad Management System";
    public string ApiToken { get; set; } = string.Empty; // Para Mailtrap API
    public string ApiUrl { get; set; } = "https://send.api.mailtrap.io/api/send"; // Mailtrap API endpoint
    public bool UseApi { get; set; } = false; // true = usar API, false = usar SMTP
}