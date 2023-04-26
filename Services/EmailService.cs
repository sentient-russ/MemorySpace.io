using ms.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


namespace ms.Services
{
    public class EmailService
    {

        public String SendContactMessage(ContactDataModel complexDataIn)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("cs@magnadigi.com"));
            email.To.Add(MailboxAddress.Parse("cs@magnadigi.com"));
            email.Subject = "MemorySpace.io Contact Priority Level: High";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = "<p><strong>Contact Name: </strong>" + complexDataIn.Name + "</p>" +
              "<p><strong>Business Name: </strong>" + complexDataIn.Business + "</p>" +
              "<p><strong>Contact Number: </strong>" + complexDataIn.Phone + "</p>" +
              "<p><strong>Contact Email: </strong>" + complexDataIn.Email + "</p>" +
              "<p><strong>Message: </strong>" + complexDataIn.Message + "</p>"
            };
            using var smtp = new SmtpClient();
            smtp.Connect("us2.smtp.mailhostbox.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("cs@magnadigi.com", "#WrncUkPJ3");
            var response = smtp.Send(email);
            smtp.Disconnect(true);
            return response;
        }
    }
}
