using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        public string? Review { get; set; }
        public int? Rating { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? UploadedAt { get; set; }
        public User? User { get; set; }
        public Movie? Movie { get; set; }
    }
}
