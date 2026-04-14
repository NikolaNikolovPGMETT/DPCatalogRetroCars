using Microsoft.EntityFrameworkCore;
using DPCatalogRetroCars.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DPCatalogRetroCars.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<CarStory> CarStories => Set<CarStory>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<FavoriteCar> FavoriteCars => Set<FavoriteCar>();
        public DbSet<RetroEvent> RetroEvents => Set<RetroEvent>();
        public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Rating>()
                .HasIndex(r => new { r.CarStoryId, r.UserId })
                .IsUnique();

            builder.Entity<FavoriteCar>()
                .HasIndex(f => new { f.CarStoryId, f.UserId })
                .IsUnique();

            builder.Entity<EventRegistration>()
                .HasIndex(er => new { er.RetroEventId, er.UserId })
                .IsUnique();
        }
    }
}
