using Microsoft.EntityFrameworkCore;
using MovieTicket.Models;

namespace MovieTicket.Data
{
    public class MovieTicketContext : DbContext
    {
        public MovieTicketContext (DbContextOptions<MovieTicketContext> options) : base(options) { }
        public DbSet<MovieTicket.Models.Movie> Movies { get; set;}
        public DbSet<MovieTicket.Models.User> Users { get; set;}
        public DbSet<MovieTicket.Models.Role> Roles { get; set;}
        public DbSet<MovieTicket.Models.Feedback> Feedbacks { get; set; }
        public DbSet<MovieTicket.Models.RatingReviewViewModel> RatingReviewViewModel { get; set; } = default!;
    }
}