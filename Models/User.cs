using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public long? Mobile { get; set; }
        public string? Password { get; set; }

    }
}
