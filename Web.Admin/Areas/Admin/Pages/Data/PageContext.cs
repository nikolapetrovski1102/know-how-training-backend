using Microsoft.EntityFrameworkCore;
using Areas.Admin.Models;

namespace Areas.Admin.Data
{
    public class PageContext : DbContext
    {
        public DbSet<PageEntity> Pages { get; set; } = null!;

        public PageContext(DbContextOptions<PageContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PageEntity>()
                .Property(p => p.ContentJson)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<PageEntity>()
                .HasIndex(p => new { p.Slug, p.Language });
        }
    }

    public class PageEntity
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Language { get; set; } = LanguageCode.MK;
        public string ContentJson { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }
}
