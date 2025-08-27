using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecommerce.Infrastructure.Models;

namespace Ecommerce.Infrastructure.Services
{
    using Ecommerce.Application.Interfaces;

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings, IUserRepository userRepository, IEmailSender? emailSender = null)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
            _userRepository = userRepository;
            _emailSender = emailSender ?? new SmtpEmailSender();
        }

        public async Task SendOrderConfirmationAsync(Guid userId, Guid orderId, decimal totalAmount, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("[EmailService] Iniciando envio de confirmação de pedido. UserId: {UserId}, OrderId: {OrderId}, Total: {Total}", userId, orderId, totalAmount);

                if (string.IsNullOrEmpty(_emailSettings.SmtpUsername) || string.IsNullOrEmpty(_emailSettings.SmtpPassword))
                {
                    _logger.LogWarning("[EmailService] Configurações de email não definidas. Apenas logando o envio.");
                    _logger.LogInformation("[EmailService] Enviando confirmação de pedido. UserId: {UserId}, OrderId: {OrderId}, Total: {Total}", userId, orderId, totalAmount);
                    return;
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogError("[EmailService] Usuário não encontrado. UserId: {UserId}", userId);
                    return;
                }

                var userEmail = user.Email;
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("[EmailService] Email do usuário não encontrado. UserId: {UserId}", userId);
                    return;
                }

                // Ajuste para provedores como Gmail: o From deve ser o mesmo do SMTP_USERNAME
                var normalizedFrom = string.IsNullOrWhiteSpace(_emailSettings.FromEmail)
                    ? _emailSettings.SmtpUsername
                    : _emailSettings.FromEmail;
                var isGmail = (_emailSettings.SmtpServer ?? string.Empty).Contains("gmail.com", StringComparison.OrdinalIgnoreCase)
                              || (_emailSettings.SmtpUsername ?? string.Empty).EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase);
                if (isGmail && !string.Equals(normalizedFrom, _emailSettings.SmtpUsername, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[EmailService] Forçando FromEmail = SMTP_USERNAME por política do Gmail.");
                    normalizedFrom = _emailSettings.SmtpUsername;
                }

                var effectiveSettings = new EmailSettings
                {
                    SmtpServer = _emailSettings.SmtpServer,
                    SmtpPort = _emailSettings.SmtpPort,
                    SmtpUsername = _emailSettings.SmtpUsername,
                    SmtpPassword = _emailSettings.SmtpPassword,
                    FromEmail = normalizedFrom,
                    FromName = string.IsNullOrWhiteSpace(_emailSettings.FromName) ? "E-commerce" : _emailSettings.FromName,
                    EnableSsl = _emailSettings.EnableSsl
                };

                _logger.LogInformation("[EmailService] Enviando via SMTP host={Host}, port={Port}, ssl={Ssl}, from={From}", effectiveSettings.SmtpServer, effectiveSettings.SmtpPort, effectiveSettings.EnableSsl, effectiveSettings.FromEmail);

                var subject = "Confirmação de Pedido - E-commerce";
                var body = GenerateOrderConfirmationEmail(user, orderId, totalAmount);

                await _emailSender.SendAsync(effectiveSettings, userEmail, subject, body, cancellationToken);

                _logger.LogInformation("[EmailService] Email de confirmação enviado com sucesso para {Email}", userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] Erro ao enviar email de confirmação. UserId: {UserId}, OrderId: {OrderId}", userId, orderId);
                throw;
            }
        }

        private string GenerateOrderConfirmationEmail(Ecommerce.Domain.Entities.User user, Guid orderId, decimal totalAmount)
        {
            var userName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrEmpty(userName))
                userName = "Cliente";

            return $@"
                <html>
                <body>
                    <p>Olá {userName}!</p>
                    <p>Seu pedido {orderId} foi processado no valor de R$ {totalAmount:F2}.</p>
                </body>
                </html>";
        }
    }

    internal sealed class SmtpEmailSender : IEmailSender
    {
        public async Task SendAsync(EmailSettings settings, string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            using var client = new System.Net.Mail.SmtpClient(settings.SmtpServer, settings.SmtpPort)
            {
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                EnableSsl = settings.EnableSsl,
                Credentials = new System.Net.NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                Timeout = 30000
            };

            using var message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(settings.FromEmail, settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
