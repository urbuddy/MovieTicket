using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class SignInSignUpViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter first name")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "Please enter email")]
        [Remote(action: "EmailIsNotExists",controller: "Movies")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Please enter mobile number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Please enter valid mobile number")]
        [Display(Name = "Mobile Number")]
        public long? Mobile { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Please enter confirm password")]
        [Compare("Password", ErrorMessage = "Confirm Password not matched!")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
