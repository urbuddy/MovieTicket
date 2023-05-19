using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Remote(action: "RoleIsExists", controller: "Movies")]
        public string? Name { get; set; }
    }
}
