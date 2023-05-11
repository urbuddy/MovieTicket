using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "Please enter email")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string? Password { get; set; }
    }
}
