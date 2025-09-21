using Microsoft.EntityFrameworkCore;

namespace TweetFi.Data
{
    public class TwitterDbContext : DbContext
    {
        public TwitterDbContext(DbContextOptions<TwitterDbContext> options) : base(options) { }

        public DbSet<TwitterState> TwitterStates => Set<TwitterState>();
    }
}
