using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SendGrid;
using SendGrid.Helpers.Mail;
using ms.Models;
using ms.Services;

namespace ms.Controllers
{

    [ApiController]
    [Route("[Controller]")]
    [BindProperties(SupportsGet = true)]

    public class ContactController : Controller
    {

        [StringLength(50, MinimumLength = 2)]
        [DisplayName("Full Name:")]
        [BindProperty(SupportsGet = true, Name = "Name")]
        public string? Name { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [DisplayName("Business Name:")]
        [BindProperty(SupportsGet = true, Name = "Business")]
        public string? Business { get; set; }

        [StringLength(50, MinimumLength = 6)]
        [DisplayName("Phone Number:")]
        [BindProperty(SupportsGet = true, Name = "Phone")]
        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(255, MinimumLength = 6)]
        [DisplayName("Email Address:")]
        [BindProperty(SupportsGet = true, Name = "Email")]
        public string? Email { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(255, MinimumLength = 1)]
        [DisplayName("Message:")]
        [BindProperty(SupportsGet = true, Name = "Message")]
        public string? Message { get; set; }


        [HttpPost]
        public IActionResult PostAsync([FromForm] ContactDataModel ComplexDataIn)
        {
            EmailService emailService = new EmailService();
            string response = emailService.SendContactMessage(ComplexDataIn);
            bool messageSent;
            if (response.Contains("Ok"))
            {
                messageSent = true;
            }
            else
            {
                messageSent = false;
            }
            ViewData["wasSent"] = messageSent;
            return RedirectToAction("Index", "Home");
        }
    }
}
