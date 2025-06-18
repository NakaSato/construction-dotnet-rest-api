using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> SendEmailAsync(string to, string subject, string body, string? cc = null)
    {
        try
        {
            // In a real implementation, you would use an email service like SendGrid, AWS SES, etc.
            // For now, we'll just log the email and simulate success
            
            _logger.LogInformation($"Sending email to: {to}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Body: {body}");
            
            if (!string.IsNullOrEmpty(cc))
            {
                _logger.LogInformation($"CC: {cc}");
            }

            // Simulate email sending delay
            await Task.Delay(100);

            // TODO: Implement actual email sending logic here
            // Example with SendGrid:
            /*
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);
            
            if (!string.IsNullOrEmpty(cc))
            {
                msg.AddCc(new EmailAddress(cc));
            }
            
            var response = await client.SendEmailAsync(msg);
            return new ApiResponse<bool>
            {
                Success = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Email sent successfully" : "Failed to send email"
            };
            */

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Email sent successfully (simulated)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to send email"
            };
        }
    }

    public async Task<ApiResponse<bool>> SendBulkEmailAsync(List<string> recipients, string subject, string body)
    {
        try
        {
            var tasks = recipients.Select(recipient => SendEmailAsync(recipient, subject, body));
            var results = await Task.WhenAll(tasks);

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Length - successCount;

            return new ApiResponse<bool>
            {
                Success = failureCount == 0,
                Data = failureCount == 0,
                Message = $"Sent {successCount} emails successfully, {failureCount} failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to send bulk email"
            };
        }
    }
}
