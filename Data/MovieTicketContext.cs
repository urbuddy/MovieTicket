using Microsoft.EntityFrameworkCore;

namespace MovieTicket.Data
{
    public class MovieTicketContext : DbContext
    {
        public MovieTicketContext (DbContextOptions<MovieTicketContext> options) : base(options) { }
        public DbSet<MovieTicket.Models.Movie> Movies { get; set;}
        public DbSet<MovieTicket.Models.User> Users { get; set;}
    }
}