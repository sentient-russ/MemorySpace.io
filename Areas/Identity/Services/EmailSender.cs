using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;

namespace WebPWrecover.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private string MS_Email_Pass = Environment.GetEnvironmentVariable("MS_Email_Pass");
    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;

    }
    public AuthMessageSenderOptions Options { get; }
    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        await Execute(subject, message, toEmail);
    }
    public async Task Execute(string subject, string message, string toEmail)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("cs@magnadigi.com"));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };
        using var smtp = new SmtpClient();
        smtp.Connect("us2.smtp.mailhostbox.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate("cs@magnadigi.com", MS_Email_Pass);
        var response = smtp.Send(email);
        smtp.Disconnect(true);
        _logger.LogInformation("The message smtp send to " + toEmail + "was attempted and returned a status of: " + response);
    }
}



