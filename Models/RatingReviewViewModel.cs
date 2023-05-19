using System.ComponentModel.DataAnnotations;

namespace MovieTicket.Models
{
    public class RatingReviewViewModel
    {
        public int Id { get; set; }
        public string? Review { get; set; }
        public int? Rating { get; set; }
        public User? Users { get; set; }
        public Movie? Movies { get; set; }
    }
}
