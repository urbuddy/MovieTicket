using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        public string? Email { get; set; }
        [Display(Name = "Mobile No.")]
        public long? Mobile { get; set; }
        public string? Password { get; set; }
        public Role? Role { get; set; }
    }
}
